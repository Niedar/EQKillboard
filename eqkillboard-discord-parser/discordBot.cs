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
            if (message.Channel.Name.IndexOf("yellowtext", StringComparison.OrdinalIgnoreCase) >= 0)
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
            var killmailChannel = killmailGuild.TextChannels.FirstOrDefault(x => x.Name.IndexOf("yellowtext", StringComparison.OrdinalIgnoreCase) >= 0);

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
            var messageLimit = 100;

            while(serverTime - lastRetrievedDiscordMsg.CreatedAt < historyLengthSetting)
            {
                    var messages = await killmailChannel.GetMessagesAsync(lastRetrievedDiscordMsg.Id, Direction.Before, messageLimit).Flatten();
                    
                    if (messages.Count() == 0)
                        break;

                    foreach (var message in messages) {
                        await ProcessMessage(message);
                        await Task.Delay(200);
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
            KillmailModel parsedKillmail = null;
            Killmail insertedKillmail = null;

            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {

                connection.Open();

                using (var killmailTransaction = connection.BeginTransaction()) {
                    try
                    {
                        rawKillMailId = await InsertRawKillmailAsync(connection, message);

                        // Parse raw killmail
                        parsedKillmail = killmailParser.ExtractKillmail(message.Content);
                        parsedKillmail.killmail_raw_id = rawKillMailId;

                        insertedKillmail = await InsertParsedKillmailAsync(connection, parsedKillmail);

                        killmailTransaction.Commit();                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        killmailTransaction.Rollback();
                    }
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
                if (!string.IsNullOrEmpty(victim.classLevel))
                {
                    await InsertClassLevel(connection, victim, insertedKillmail, message);
                }
                if (!string.IsNullOrEmpty(attacker.classLevel))
                {
                    await InsertClassLevel(connection, attacker, insertedKillmail, message);  
                } 
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

            await connection.ExecuteAsync(sql, parameters);
            
            var killmailRawId =  parameters.Get<int>("KillmailRawId");
            return killmailRawId;
        }

        private async Task<Killmail> InsertParsedKillmailAsync(IDbConnection connection, KillmailModel parsedKillmailModel) 
        {
            var killmailToInsert = new Killmail();

            CultureInfo USCultureInfo = new CultureInfo("en-US");
            var timezone = "America/Chicago";
            var killedAtLocalTime = DateTime.SpecifyKind(DateTime.Parse(parsedKillmailModel.killedAt, USCultureInfo), DateTimeKind.Unspecified);
            
            killmailToInsert.killed_at = killedAtLocalTime.InZone(timezone);
            killmailToInsert.killmail_raw_id = parsedKillmailModel.killmail_raw_id;
            killmailToInsert.victim_guild_id = await GetOrInsertGuild(connection, parsedKillmailModel.victimGuild);
            killmailToInsert.attacker_guild_id = await GetOrInsertGuild(connection, parsedKillmailModel.attackerGuild);
            killmailToInsert.zone_id = await GetOrInsertZone(connection, parsedKillmailModel.zone);
            killmailToInsert.victim_id =  await GetOrInsertCharacter(connection, parsedKillmailModel.victimName, killmailToInsert.victim_guild_id);
            killmailToInsert.attacker_id = await GetOrInsertCharacter(connection, parsedKillmailModel.attackerName, killmailToInsert.attacker_guild_id);

            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(new {
                killed_at = killmailToInsert.killed_at,
                killmail_raw_id = killmailToInsert.killmail_raw_id,
                victim_guild_id = killmailToInsert.victim_guild_id,
                attacker_guild_id = killmailToInsert.attacker_guild_id,
                zone_id = killmailToInsert.zone_id,
                victim_id = killmailToInsert.victim_id,
                attacker_id = killmailToInsert.attacker_id
            });

            // Finally, insert killmail 
            var killmailInsertSql = @"INSERT INTO killmail (victim_id, victim_guild_id, attacker_id, attacker_guild_id, zone_id, killed_at, killmail_raw_id)
                        VALUES (@victim_id, @victim_guild_id, @attacker_id, @attacker_guild_id, @zone_id, @killed_at, @killmail_raw_id)
                        RETURNING id;
                        ";

            await connection.ExecuteAsync(killmailInsertSql, dynamicParams);
            return killmailToInsert;
        }


        private async Task<int?> GetOrInsertGuild(IDbConnection connection, string name)
        {
            DynamicParameters parameters = new DynamicParameters();
            
            // First, check if victim guild name is empty and then insert query for victim guild
            if (String.IsNullOrEmpty(name)) {
                return null;
            }
            else 
            {
                var guildSelectQuery = @"SELECT id FROM guild WHERE name = @Name;";
                var guild_id = await connection.ExecuteScalarAsync<int?>(guildSelectQuery, new { Name = name});
                if (guild_id != null)
                {
                    return guild_id.Value;
                }
                
                var victimGuildInsertSql = @"INSERT INTO guild (name) 
                                        VALUES (@VictimGuild)
                                        ON CONFLICT(name) DO UPDATE 
                                        SET name = EXCLUDED.name 
                                        RETURNING id;
                                        ";

                parameters.Add("@VictimGuild", name);
                parameters.Add("@VictimGuildId", direction: ParameterDirection.Output);

                await connection.ExecuteAsync(victimGuildInsertSql, parameters);
                return parameters.Get<int>("VictimGuildId");
            }            
        }

        private async Task<int> GetOrInsertZone(IDbConnection connection, string name)
        {
            var zoneSelectQuery = @"SELECT id FROM zone WHERE name = @Name;";
            var zoneId = await connection.ExecuteScalarAsync<int?>(zoneSelectQuery, new { Name = name});
            if (zoneId != null)
            {
                return zoneId.Value;
            }

            var parameters = new DynamicParameters();
            var zoneInsertSql = @"INSERT INTO zone (name) 
                        VALUES (@ZoneName)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name 
                        RETURNING id;
                        ";

            parameters.Add("@ZoneName", name);
            parameters.Add("@ZoneId", direction: ParameterDirection.Output);

            await connection.ExecuteAsync(zoneInsertSql, parameters);
            return parameters.Get<int>("ZoneId");
        }

        private async Task<int> GetOrInsertCharacter(IDbConnection connection, string name, int? guildId)
        {
            var charSelectQuery = @"SELECT id FROM character WHERE name = @Name;";
            var characterId = await connection.ExecuteScalarAsync<int?>(charSelectQuery, new { Name = name});
            if (characterId != null)
            {
                return characterId.Value;
            }

            var parameters = new DynamicParameters();
            var victimCharInsertSql = @"INSERT INTO character (name, guild_id) 
                        VALUES (@VictimName, @GuildId)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name,
                        guild_id = EXCLUDED.guild_id
                        RETURNING id;
                        ";

            parameters.Add("@VictimName", name);
            parameters.Add("@GuildId", guildId);
            parameters.Add("@VictimId", direction: ParameterDirection.Output);

            await connection.ExecuteAsync(victimCharInsertSql, parameters);
            return parameters.Get<int>("VictimId");            
        }

        private async Task InsertClassLevel(IDbConnection connection, CharacterModel character, Killmail insertedKillmail, IMessage message) {

            // Initialize variables for time testing as a base for relevance of data
            var historyLengthUpdateSetting = new TimeSpan(0, 12, 0, 0);
            var serverTime = new DateTimeOffset(DateTime.Now);
            
            var classLevelParser = new KillMailParser();
            
            var levelString = classLevelParser.ExtractLevel(character.classLevel);
            if (!string.IsNullOrEmpty(levelString))
            {
                character.level = Convert.ToInt32(levelString);
            }
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

            if (!string.IsNullOrEmpty(character.className))
            {
                var classSelectQuery = @"SELECT id FROM class WHERE name = @Name;";
                var classId = await connection.ExecuteScalarAsync<int?>(classSelectQuery, new { Name = character.className});
                if (classId != null)
                {
                    character.classId = classId.Value;
                }
                else
                {
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
                        await connection.ExecuteAsync(insertClassSql, parameters);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }

                    character.classId = parameters.Get<int>("ClassId");
                }
            }

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