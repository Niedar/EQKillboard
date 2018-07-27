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

namespace eqkillboard_discord_parser
{
    public class Program
    {
        private DiscordSocketClient client;
        private DiscordSocketConfig config;
        private string DbConnectionString;
        //public string DbConnectionString { get; set; }
        public static void Main(string[] args) {
            new Program().MainAsync().GetAwaiter().GetResult();
        }
 
        public async Task MainAsync()
        {

            // add json config for DBsettings, token, etc.
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            var configuration = builder.Build(); 

            // call connection string from config
            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];

            // create Config for Discord
            config = new DiscordSocketConfig {
                MessageCacheSize = 100
            };

            config.WebSocketProvider = WS4NetProvider.Instance; // Only needed for Win 7, remove otherwise

            client = new DiscordSocketClient(config);
            client.Log += Log;
 
            string token = configuration["Tokens:userToken"];
            await client.LoginAsync(TokenType.User, token);
            await client.StartAsync();
            client.Ready += GetHistory;
        
            client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
 
        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Channel.Name == "yellowtext")
            {
                await InsertRawKillmailAsync(message.Id, message.Content);
            }
        }
 
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task GetHistory()
        {
            Console.WriteLine(config.MessageCacheSize);

            var killmailGuild = client.Guilds.FirstOrDefault(x => x.Name == "Rise of Zek");
            
            var killmailChannel = killmailGuild.TextChannels.FirstOrDefault(x => x.Name == "yellowtext");

            if (killmailChannel != null)
                {
                    KillMailParser killmailParser = new KillMailParser();
                    var messageLimit = 5;
                    var rawKillMailId = 0;

                    killmailChannel.GetMessagesAsync(messageLimit)
                    .ForEach(async m => {
                        foreach(var message in m) {
                            try {
                                rawKillMailId = await InsertRawKillmailAsync(message.Id, message.Content);
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex);
                            }
                            var extractedKillmail = killmailParser.ExtractKillmail(message.Content);
                            extractedKillmail.killmail_raw_id = rawKillMailId;

                            var killmailToInsert = await InsertParsedKillmailAsync(extractedKillmail);

                            // Get level and class for each char
                            var scraper = new Scraper();
                            var victimClassLevel = await scraper.ScrapeCharInfo(extractedKillmail.victimName);
                            var attackerClassLevel = await scraper.ScrapeCharInfo(extractedKillmail.attackerName); 

                            // MAKE CHECK FOR ATTACKER OR VICTIM LEVEL
                            // await InsertClassLevel(extractedKillmail, victimClassLevel);
                            // await InsertClassLevel(extractedKillmail, victimClassLevel);
                        }
                        });

                }
			return Task.CompletedTask;
        }
 
        private async Task<int> InsertRawKillmailAsync(UInt64 messageId, string message)
        {
            var messageIdSigned = Convert.ToInt64(messageId);
            Console.WriteLine(message.ToString());

            var connection = DatabaseConnection.CreateConnection(DbConnectionString);

            DynamicParameters parameters = new DynamicParameters();

            var sql = @"INSERT INTO killmail_raw (discord_message_id, message) Values (@MessageId, @Message)
           ON CONFLICT (discord_message_id) DO UPDATE
           SET message = EXCLUDED.message
           RETURNING id
           "; // insert raw killmail

            parameters.Add("@MessageId", messageIdSigned);
            parameters.Add("@Message", message);

            parameters.Add("@KillmailRawId", direction: ParameterDirection.Output);

            try {
                await connection.ExecuteAsync(sql, parameters);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            
            var killmailRawId =  parameters.Get<int>("KillmailRawId");

            //connection.Close();

            return killmailRawId;
        }

        private async Task<Killmail> InsertParsedKillmailAsync(KillmailModel parsedKillmailModel) {
            var connection = DatabaseConnection.CreateConnection(DbConnectionString);

            var killmailToInsert = new Killmail();

            // TODO: convert killedAt property to DateTime
            CultureInfo USCultureInfo = new CultureInfo("en-US");
            killmailToInsert.killed_at = DateTime.Parse(parsedKillmailModel.killedAt, USCultureInfo);
            killmailToInsert.killmail_raw_id = parsedKillmailModel.killmail_raw_id;

            DynamicParameters parameters = new DynamicParameters();

            // maybe refactor all these queries at some point!

            // First, insert query for victim guild
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

            killmailToInsert.victim_guild_id =  parameters.Get<int>("VictimGuildId");

            // Reset params, next: Attacker guild
            parameters = new DynamicParameters();

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

            // Set mock value for level for testing and preparation purposes - victim level
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

            // Set mock value for level for testing and preparation purposes - attacker level

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

            connection.Close();

            return killmailToInsert;
        }

        private async Task<Killmail> InsertClassLevel(Killmail killmailToInsert, string classLevel) {
            var connection = DatabaseConnection.CreateConnection(DbConnectionString);
            var classLevelParser = new KillMailParser();
            
            var level = classLevelParser.ExtractLevel(classLevel);
            var className = classLevelParser.ExtractChar(classLevel);

            var insertLevelSql = @"INSERT INTO character (level) 
                        VALUES (@Level)
                        ON CONFLICT(level) DO UPDATE 
                        SET level = EXCLUDED.level,
                        RETURNING id;
                        ";

            try {
                await connection.ExecuteAsync(insertLevelSql, level);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            var insertClassSql = @"INSERT INTO class (name) 
                                VALUES (@className)
                                ON CONFLICT(name) DO UPDATE 
                                SET name = EXCLUDED.name,
                                RETURNING id;
                                ";

            try {
                await connection.ExecuteAsync(insertClassSql, className);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            var insertLevelIntoKillmailSql = @"INSERT INTO killmail (name) 
                                VALUES (@className)
                                ON CONFLICT(name) DO UPDATE 
                                SET name = EXCLUDED.name,
                                RETURNING id;
                                ";

            try {
                await connection.ExecuteAsync(insertLevelIntoKillmailSql, level);
            }

            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            connection.Close();

            return killmailToInsert;
        }
    }
}