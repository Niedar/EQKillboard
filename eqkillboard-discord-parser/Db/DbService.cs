using System.IO;
using Microsoft.Extensions.Configuration;
using Dapper;
using EQKillboard.DiscordParser.Entities;
using System.Threading.Tasks;
using EQKillboard.DiscordParser.Models;
using Discord;
using System;
using System.Data;
using System.Globalization;

namespace EQKillboard.DiscordParser.Db
{
    public class DbService
    {
        private IConfiguration configuration;
        private string DbConnectionString;
        private int Season;
        public DbService()
        {
            // add json config for DBsettings, token, etc.
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            configuration = builder.Build(); 

            // call connection string from config
            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];
            Season = Convert.ToInt32(configuration["Settings:Season"]);
        }

        public async Task<Killmail> InsertRawAndParsedKillMailAsync(IMessage discordMessage, ParsedKillMail parsedKillMail)
        {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                connection.Open();
                using (var killmailTransaction = connection.BeginTransaction()) {
                    try
                    {
                        var rawKillMail = await InsertRawKillmailAsync(connection, discordMessage);                       
                        parsedKillMail.KillMailRawId = rawKillMail.id;

                        var insertedKillmail = await InsertParsedKillmailAsync(connection, parsedKillMail);
                        foreach(var involved in parsedKillMail.Involved)
                        {
                            insertedKillmail.Involved.Add(await InsertParsedKillMailInvolved(connection, insertedKillmail, involved));
                        }

                        killmailTransaction.Commit();

                        return insertedKillmail;                      
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        killmailTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task InsertMissingKillMailInvolvedAsync(IMessage discordMessage, ParsedKillMail parsedKillMail)
        {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) 
            {
                connection.Open();
                using (var killmailTransaction = connection.BeginTransaction()) {
                    try
                    {
                        var killmail = await GetKillmailAsync(discordMessage.Id);
                        if (killmail != null && parsedKillMail.Involved != null)
                        {
                            foreach (var involved in parsedKillMail.Involved)
                            {
                                await InsertParsedKillMailInvolved(connection, killmail, involved);
                            }
                        }
                        killmailTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        killmailTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task<RawKillMail> InsertRawKillmailAsync(IDbConnection connection, IMessage discordMessage)
        {
            Console.WriteLine(discordMessage.Content.ToString());
            DynamicParameters parameters = new DynamicParameters();
            
            var sql = @"INSERT INTO killmail_raw (discord_message_id, message, season) Values (@MessageId, @Message, @Season)
            ON CONFLICT (discord_message_id) DO UPDATE
            SET message = EXCLUDED.message
            RETURNING id, discord_message_id, message, status_type_id
            "; // insert raw killmail

            decimal messageId = discordMessage.Id;
            parameters.Add("@MessageId", messageId);
            parameters.Add("@Message", discordMessage.Content);
            parameters.Add("@Season", Season);

            return await connection.QueryFirstAsync<RawKillMail>(sql, parameters);
        }

        private async Task<Killmail> InsertParsedKillmailAsync(IDbConnection connection, ParsedKillMail parsedKillmailModel) 
        {
            var killmailToInsert = new Killmail();

            CultureInfo USCultureInfo = new CultureInfo("en-US");
            var timezone = "America/Chicago";

            // Server changed from chicago time to UTC time instead so this is not being used for now
            //var killedAtLocalTime = DateTime.SpecifyKind(DateTime.Parse(parsedKillmailModel.killedAt, USCultureInfo), DateTimeKind.Unspecified);
            //killedAtLocalTime.InZone(timezone);

            var killedAtLocalTime = DateTime.SpecifyKind(DateTime.Parse(parsedKillmailModel.KilledAt, USCultureInfo), DateTimeKind.Utc);
            
            killmailToInsert.killed_at = killedAtLocalTime;
            killmailToInsert.killmail_raw_id = parsedKillmailModel.KillMailRawId;
            killmailToInsert.victim_guild_id = await GetOrInsertGuild(connection, parsedKillmailModel.VictimGuild);
            killmailToInsert.attacker_guild_id = await GetOrInsertGuild(connection, parsedKillmailModel.AttackerGuild);
            killmailToInsert.zone_id = await GetOrInsertZone(connection, parsedKillmailModel.Zone);
            killmailToInsert.victim_id =  await GetOrInsertCharacter(connection, parsedKillmailModel.VictimName, parsedKillmailModel.VictimIsNpc, killmailToInsert.victim_guild_id, parsedKillmailModel.VictimLevel, parsedKillmailModel.VictimClass);
            killmailToInsert.attacker_id = await GetOrInsertCharacter(connection, parsedKillmailModel.AttackerName, parsedKillmailModel.AttackerIsNpc, killmailToInsert.attacker_guild_id, parsedKillmailModel.AttackerLevel, parsedKillmailModel.AttackerClass);
            killmailToInsert.victim_level = parsedKillmailModel.VictimLevel;
            killmailToInsert.attacker_level = parsedKillmailModel.AttackerLevel;

            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(new {
                killed_at = killmailToInsert.killed_at,
                killmail_raw_id = killmailToInsert.killmail_raw_id,
                victim_guild_id = killmailToInsert.victim_guild_id,
                attacker_guild_id = killmailToInsert.attacker_guild_id,
                zone_id = killmailToInsert.zone_id,
                victim_id = killmailToInsert.victim_id,
                attacker_id = killmailToInsert.attacker_id,
                victim_level = killmailToInsert.victim_level,
                attacker_level = killmailToInsert.attacker_level,
                season = Season
            });
            dynamicParams.Add("@KillMailId", direction: ParameterDirection.Output);

            // Finally, insert killmail 
            var killmailInsertSql = 
            @"INSERT INTO killmail (victim_id, victim_guild_id, attacker_id, attacker_guild_id, zone_id, killed_at, killmail_raw_id, victim_level, attacker_level, season)
            VALUES (@victim_id, @victim_guild_id, @attacker_id, @attacker_guild_id, @zone_id, @killed_at, @killmail_raw_id, @victim_level, @attacker_level, @season)
            RETURNING id;
            ";

            await connection.ExecuteAsync(killmailInsertSql, dynamicParams);
            killmailToInsert.id = dynamicParams.Get<int>("KillMailId");
            return killmailToInsert;
        }

        private async Task<KillmailInvolved> InsertParsedKillMailInvolved(IDbConnection connection, Killmail killMail, ParsedKillMailInvolved involved)
        {
            var killMailInvolvedToInsert = new KillmailInvolved();
            killMailInvolvedToInsert.killmail_id = killMail.id;
            killMailInvolvedToInsert.attacker_guild_id = await GetOrInsertGuild(connection, involved.AttackerGuild);
            killMailInvolvedToInsert.attacker_level = involved.AttackerLevel;
            killMailInvolvedToInsert.attacker_id = await GetOrInsertCharacter(connection, involved.AttackerName, involved.AttackerIsNpc, killMailInvolvedToInsert.attacker_guild_id, involved.AttackerLevel, involved.AttackerClass);
            killMailInvolvedToInsert.melee_damage = involved.MeleeDamage;
            killMailInvolvedToInsert.melee_hits = involved.MeleeHits;
            killMailInvolvedToInsert.spell_damage = involved.SpellDamage;
            killMailInvolvedToInsert.spell_hits = involved.SpellHits;
            killMailInvolvedToInsert.dispel_slots = involved.DispelSlots;

            var insertQuery = 
            @"INSERT INTO killmail_involved (killmail_id, attacker_id, attacker_guild_id, attacker_level, melee_damage, melee_hits, spell_damage, spell_hits, dispel_slots)
            VALUES (@killmail_id, @attacker_id, @attacker_guild_id, @attacker_level, @melee_damage, @melee_hits, @spell_damage, @spell_hits, @dispel_slots)
            ON CONFLICT (killmail_id, attacker_id) DO NOTHING;
            ";

            await connection.ExecuteAsync(insertQuery, killMailInvolvedToInsert);

            return killMailInvolvedToInsert;
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
                var guildSelectQuery = @"SELECT id FROM guild WHERE name = @Name AND season = @Season;";
                var guild_id = await connection.ExecuteScalarAsync<int?>(guildSelectQuery, new { Name = name, Season});
                if (guild_id != null)
                {
                    return guild_id.Value;
                }
                
                var victimGuildInsertSql = @"INSERT INTO guild (name, season) 
                                        VALUES (@VictimGuild, @Season)
                                        ON CONFLICT(name, season) DO UPDATE 
                                        SET name = EXCLUDED.name,
                                        season = EXCLUDED.season
                                        RETURNING id;
                                        ";

                parameters.Add("@VictimGuild", name);
                parameters.Add("@Season", Season);
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

        private async Task<int> GetOrInsertClass(IDbConnection connection, string name)
        {
            var classSelectQuery = @"SELECT id FROM class WHERE name = @Name;";
            var classId = await connection.ExecuteScalarAsync<int?>(classSelectQuery, new { Name = name});
            if (classId != null)
            {
                return classId.Value;
            }

            var parameters = new DynamicParameters();
            var classInsertSql = @"INSERT INTO class (name) 
                        VALUES (@ClassName)
                        ON CONFLICT(name) DO UPDATE 
                        SET name = EXCLUDED.name 
                        RETURNING id;
                        ";

            parameters.Add("@ClassName", name);
            parameters.Add("@ClassId", direction: ParameterDirection.Output);

            await connection.ExecuteAsync(classInsertSql, parameters);
            return parameters.Get<int>("ClassId");
        }

        private async Task<int> GetOrInsertCharacter(IDbConnection connection, string name, Boolean isNpc, int? guildId, int? level, string className)
        {
            int? classId = null;
            if (!string.IsNullOrEmpty(className))
            {
                classId = await GetOrInsertClass(connection, className);
            }

            var parameters = new DynamicParameters();
            
            var charSelectQuery = @"SELECT id FROM character WHERE name = @Name AND season = @Season;";
            var characterId = await connection.ExecuteScalarAsync<int?>(charSelectQuery, new { Name = name, Season});
            if (characterId != null)
            {
                var charUpdateQuery = @"UPDATE character SET is_npc = @IsNpc, guild_id = @GuildId, level = @Level, class_id = @ClassId WHERE id = @Id";
                parameters.Add("@Id", characterId.Value);
                parameters.Add("@IsNpc", isNpc);
                parameters.Add("@GuildId", guildId);
                parameters.Add("@Level", level);
                parameters.Add("@ClassId", classId);
                
                await connection.ExecuteAsync(charUpdateQuery, parameters);
                return characterId.Value;
            }
            else
            {
                var victimCharInsertSql = @"INSERT INTO character (name, is_npc, guild_id, level, class_id, season) 
                            VALUES (@VictimName, @IsNpc, @GuildId, @Level, @ClassId, @Season)
                            ON CONFLICT(name, season) DO UPDATE 
                            SET name = EXCLUDED.name,
                            is_npc = EXCLUDED.is_npc,
                            guild_id = EXCLUDED.guild_id,
                            level = EXCLUDED.level,
                            class_id = EXCLUDED.class_id,
                            season = EXCLUDED.season
                            RETURNING id;
                            ";

                parameters.Add("@VictimName", name);
                parameters.Add("@IsNpc", isNpc);
                parameters.Add("@GuildId", guildId);
                parameters.Add("@Level", level);
                parameters.Add("@ClassId", classId);
                parameters.Add("@Season", Season);
                parameters.Add("@VictimId", direction: ParameterDirection.Output);

                await connection.ExecuteAsync(victimCharInsertSql, parameters);
                return parameters.Get<int>("VictimId");            
            }
        }

        public async Task<RawKillMail> GetRawKillMailAsync(decimal discordMessageId)
        {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                const string selectRawKillmailSql = @"
                SELECT
                    id,
                    discord_message_id,
                    message,
                    status_type_id
                FROM killmail_raw 
                WHERE discord_message_id = @messageId";
                return await connection.QueryFirstOrDefaultAsync<RawKillMail>(selectRawKillmailSql, new { messageId = discordMessageId });
            }
        }

        public async Task<Killmail> GetKillmailAsync(decimal discordMessageId)
        {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                const string selectRawKillmailSql = @"
                SELECT
                    k.id,
                    k.victim_id,
                    k.victim_guild_id,
                    k.victim_level,
                    k.attacker_id,
                    k.attacker_guild_id,
                    k.attacker_level,
                    k.zone_id,
                    k.killed_at,
                    k.killmail_raw_id,
                    k.looted_item,
                    k.looted_by,
                    k.season
                FROM killmail k
                INNER JOIN killmail_raw
                    ON killmail_raw.id = k.killmail_raw_id 
                WHERE killmail_raw.discord_message_id = @messageId";
                return await connection.QueryFirstOrDefaultAsync<Killmail>(selectRawKillmailSql, new { messageId = discordMessageId });
            }
        }
    }
}