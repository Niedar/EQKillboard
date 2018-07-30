using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System.IO;
using Microsoft.Extensions.Configuration;
using eqkillboard_discord_parser.Db;
using Dapper;
using eqkillboard_discord_parser.Entities;
using System.Data;
using System.Collections.Generic;
using eqkillboard_discord_parser.Models;
using System.Globalization;
using Npgsql;
using System.Transactions;

namespace eqkillboard_discord_parser
{
    public class Program
    {
        private IConfiguration configuration;
        private DiscordSocketClient client;
        private DiscordSocketConfig discordConfig;
        private string DbConnectionString;
        private IMessage lastRetrievedDiscordMsg;
        public static void Main(string[] args) {
            new Program().MainAsync().GetAwaiter().GetResult();
        }
 
        public async Task MainAsync()
        {

            // add json config for DBsettings, token, etc.
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            configuration = builder.Build(); 

            // call connection string from config
            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];

            // create Config for Discord
            discordConfig = new DiscordSocketConfig {
                MessageCacheSize = 100
            };

            // Only needed for Win 7, remove otherwise
            discordConfig.WebSocketProvider = WS4NetProvider.Instance; 

            client = new DiscordSocketClient(discordConfig);
            client.Log += Log;
 
            string token = configuration["Tokens:userToken"];
            await client.LoginAsync(TokenType.User, token);
            await client.StartAsync();
            client.Ready += clientReadyHandler;
        
            client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
 
        private Task MessageReceived(SocketMessage message)
        {
            if (message.Channel.Name == "yellowtext")
            {
                Task.Run(() => ProcessMessage(message));
            }
            return Task.CompletedTask;
        }
 
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task clientReadyHandler() {
            Task.Run(() => GetHistory());
            return Task.CompletedTask;
        }

        private async Task GetHistory()
        {
            var serverTime = new DateTimeOffset(DateTime.Now);

            var killmailGuild = client.Guilds.FirstOrDefault(x => x.Name == "Rise of Zek");
            var killmailChannel = killmailGuild.TextChannels.FirstOrDefault(x => x.Name == "yellowtext");

            // Retrieve first message
            if(killmailChannel != null) {
                var limit = 1;
                var messageCollToReceive = await killmailChannel.GetMessagesAsync(limit).Flatten();
                var messageToReceive = messageCollToReceive.ElementAt(0);
                lastRetrievedDiscordMsg = messageToReceive;
                await ProcessMessage(messageToReceive); // GetMessagesAsync ignores the message in the FROM parameter, so process this message now
            }

            int historyNumberOfDays;
            int.TryParse(configuration["Settings:HistoryLengthInDays"], out historyNumberOfDays);
            var historyLengthSetting = new TimeSpan(historyNumberOfDays, 0, 0, 0); // constructor with parameters: days, hours, minutes, seconds
            var messageLimit = 3;

            while(serverTime - lastRetrievedDiscordMsg.CreatedAt < historyLengthSetting)
            {
                    var messages = await killmailChannel.GetMessagesAsync(lastRetrievedDiscordMsg.Id, Direction.Before, messageLimit).Flatten();
                    
                    foreach (var message in messages) {
                        await ProcessMessage(message);
                        lastRetrievedDiscordMsg = message;
                    }
            }
        }

        private async Task ProcessMessage(IMessage message)
        {
            KillMailParser killmailParser = new KillMailParser();
            var rawKillMailId = 0;

            // Check if killmail exists
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                var selectRawKillmailSql = @"SELECT *
                                            FROM killmail_raw 
                                            WHERE discord_message_id = @messageId";

                try {
                    var messageIdSigned = Convert.ToInt64(message.Id);
                    var affectedRows = await connection.QueryAsync(selectRawKillmailSql, new { messageId = messageIdSigned });
                    
                    if (affectedRows.Count() > 0) {
                        return;
                    }
                }

                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            // Process message if nothing found
            KillmailModel parsedKillmail;
            Killmail insertedKillmail;

            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {

                connection.Open();

                using (var killmailTransaction = connection.BeginTransaction()) {

                    rawKillMailId = await InsertRawKillmailAsync(connection, message);

                    // Parse raw killmail
                    parsedKillmail = killmailParser.ExtractKillmail(message.Content);
                    parsedKillmail.killmail_raw_id = rawKillMailId;

                    insertedKillmail = await InsertParsedKillmailAsync(connection, parsedKillmail);

                    killmailTransaction.Commit();
                }
            }

            // Get level and class for each char
            var scraper = new Scraper();
            var victim = new CharacterModel{
                name = parsedKillmail.victimName,
                isAttacker = false
            };
            var attacker = new CharacterModel{
                name = parsedKillmail.attackerName,
                isAttacker = true
            };

            victim.classLevel = await scraper.ScrapeCharInfo(victim.name);
            attacker.classLevel = await scraper.ScrapeCharInfo(attacker.name); 

            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                await InsertClassLevel(connection, victim, insertedKillmail, message);
                await InsertClassLevel(connection, attacker, insertedKillmail, message);          
            }
        
        }
 
        private async Task<int> InsertRawKillmailAsync(IDbConnection connection, IMessage message)
        {
            var messageIdSigned = Convert.ToInt64(message.Id);
            Console.WriteLine(message.Content.ToString());

            DynamicParameters parameters = new DynamicParameters();
            
            var sql = @"INSERT INTO killmail_raw (discord_message_id, message) Values (@MessageId, @Message)
           ON CONFLICT (discord_message_id) DO UPDATE
           SET message = EXCLUDED.message
           RETURNING id
           "; // insert raw killmail

            parameters.Add("@MessageId", messageIdSigned);
            parameters.Add("@Message", message.Content);

            parameters.Add("@KillmailRawId", direction: ParameterDirection.Output);

            try {
                await connection.ExecuteAsync(sql, parameters);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            
            var killmailRawId =  parameters.Get<int>("KillmailRawId");

            return killmailRawId;
        }

        private async Task<Killmail> InsertParsedKillmailAsync(IDbConnection connection, KillmailModel parsedKillmailModel) {

            var killmailToInsert = new Killmail();

            CultureInfo USCultureInfo = new CultureInfo("en-US");
            killmailToInsert.killed_at = DateTime.Parse(parsedKillmailModel.killedAt, USCultureInfo);
            killmailToInsert.killmail_raw_id = parsedKillmailModel.killmail_raw_id;

            DynamicParameters parameters = new DynamicParameters();

            // maybe refactor all these queries at some point!

            // First, check if victim guild name is empty and then insert query for victim guild
            if (String.IsNullOrEmpty(parsedKillmailModel.victimGuild)) {
                killmailToInsert.victim_guild_id = null;
            }
            else {
                var victimGuildInsertSql = @"INSERT INTO guild (name) 
                                        VALUES (@VictimGuild)
                                        ON CONFLICT(name) DO UPDATE 
                                        SET name = EXCLUDED.name 
                                        RETURNING id;
                                        ";

                parameters.Add("@VictimGuild", parsedKillmailModel.victimGuild);
                parameters.Add("@VictimGuildId", direction: ParameterDirection.Output);

                try {
                    await connection.ExecuteAsync(victimGuildInsertSql, parameters);
                }

                catch (Exception ex) {
                    Console.WriteLine(ex);
                }

                killmailToInsert.victim_guild_id = parameters.Get<int>("VictimGuildId");
            }

            // Reset params, next: Attacker guild
            parameters = new DynamicParameters();

            if (String.IsNullOrEmpty(parsedKillmailModel.attackerGuild)) {
                killmailToInsert.attacker_guild_id = null;
            }
            else {
                var attackerGuildInsertSql = @"INSERT INTO guild (name) 
                                        VALUES (@AttackerGuild)
                                        ON CONFLICT(name) DO UPDATE 
                                        SET name = EXCLUDED.name 
                                        RETURNING id;
                                        ";                                    

                parameters.Add("@AttackerGuild", parsedKillmailModel.attackerGuild);
                parameters.Add("@AttackerGuildId", direction: ParameterDirection.Output);

                try {
                    await connection.ExecuteAsync(attackerGuildInsertSql, parameters);
                }

                catch (Exception ex) {
                    Console.WriteLine(ex);
                }

                killmailToInsert.attacker_guild_id =  parameters.Get<int>("AttackerGuildId");
            }


            // Reset params, next: Zone
            parameters = new DynamicParameters();

            var zoneInsertSql = @"INSERT INTO zone (name) 
                        VALUES (@ZoneName)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name 
                        RETURNING id;
                        ";

            parameters.Add("@ZoneName", parsedKillmailModel.zone);
            parameters.Add("@ZoneId", direction: ParameterDirection.Output);

            try {
                await connection.ExecuteAsync(zoneInsertSql, parameters);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            killmailToInsert.zone_id =  parameters.Get<int>("ZoneId");

            // Reset params, next: Victim name
            parameters = new DynamicParameters();

            var victimCharInsertSql = @"INSERT INTO character (name, guild_id) 
                        VALUES (@VictimName, @GuildId)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name,
                        guild_id = EXCLUDED.guild_id
                        RETURNING id;
                        ";

            parameters.Add("@VictimName", parsedKillmailModel.victimName);
            parameters.Add("@GuildId", killmailToInsert.victim_guild_id);

            parameters.Add("@VictimId", direction: ParameterDirection.Output);

            try {
                await connection.ExecuteAsync(victimCharInsertSql, parameters);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            killmailToInsert.victim_id =  parameters.Get<int>("VictimId");

            // Reset params, next: Attacker name
            parameters = new DynamicParameters();

            var attackCharInsertSql = @"INSERT INTO character (name, guild_id) 
                        VALUES (@AttackerName, @GuildId)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name,
                        guild_id = EXCLUDED.guild_id
                        RETURNING id;
                        ";

            parameters.Add("@AttackerName", parsedKillmailModel.attackerName);
            parameters.Add("@AttackerId", direction: ParameterDirection.Output);
            parameters.Add("@GuildId", killmailToInsert.attacker_guild_id);

            try {
                await connection.ExecuteAsync(attackCharInsertSql, parameters);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            killmailToInsert.attacker_id =  parameters.Get<int>("AttackerId");

            // Finally, insert killmail 
            var killmailInsertSql = @"INSERT INTO killmail (victim_id, victim_guild_id, attacker_id, attacker_guild_id, zone_id, killed_at, killmail_raw_id)
                        VALUES (@victim_id, @victim_guild_id, @attacker_id, @attacker_guild_id, @zone_id, @killed_at, @killmail_raw_id)
                        RETURNING id;
                        ";

            try {
                await connection.ExecuteAsync(killmailInsertSql, killmailToInsert);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            return killmailToInsert;
        }

        private async Task InsertClassLevel(IDbConnection connection, CharacterModel character, Killmail insertedKillmail, IMessage message) {

            // Initialize variables for time testing as a base for relevance of data
            var historyLengthUpdateSetting = new TimeSpan(0, 12, 0, 0);
            var serverTime = new DateTimeOffset(DateTime.Now);
            
            var classLevelParser = new KillMailParser();
            
            character.level = Convert.ToInt32(classLevelParser.ExtractLevel(character.classLevel));
            character.className = classLevelParser.ExtractChar(character.classLevel);

            // Update character level and killmail character level if not older than historyLengthUpdateSetting
            var insertLevelSql = @"UPDATE character 
                    SET level = @Level
                    WHERE name = @CharName
                    ";

            try {
                await connection.ExecuteAsync(insertLevelSql, new { Level = character.level, CharName = character.name });
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            if(serverTime - message.CreatedAt < historyLengthUpdateSetting) {
                string insertLevelIntoKillmailSql;

                if (character.isAttacker) {
                    insertLevelIntoKillmailSql = @"UPDATE killmail 
                                        SET attacker_level = @Level
                                        WHERE killmail_raw_id = @KillmailRawId
                                        ";
                }
                else {
                    insertLevelIntoKillmailSql = @"UPDATE killmail 
                                        SET victim_level = @Level
                                        WHERE killmail_raw_id = @KillmailRawId
                                        ";
                }

                try {
                    await connection.ExecuteAsync(insertLevelIntoKillmailSql, new { Level = character.level, KillmailRawId = insertedKillmail.killmail_raw_id});
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            // Update class table with class_id
            var insertClassSql = @"INSERT INTO class (name) 
                                VALUES (@ClassName)
                                ON CONFLICT(name) DO UPDATE
                                SET name = EXCLUDED.name
                                RETURNING id
                                ";

            var parameters = new DynamicParameters();
            parameters.Add("@ClassName", character.className);
            parameters.Add("@ClassId", direction: ParameterDirection.Output);

            try {
                character.classId = await connection.ExecuteAsync(insertClassSql, parameters);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            character.classId = parameters.Get<int>("ClassId");

            // Update character table with class_id
            var insertClassIntoCharSql = @"UPDATE character 
                                    SET class_id = @ClassId
                                    WHERE name = @CharName
                                    ";

            try {
                await connection.ExecuteAsync(insertClassIntoCharSql, new { ClassId = character.classId, CharName = character.name });
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

        }
    }
}