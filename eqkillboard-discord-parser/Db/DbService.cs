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
        public DbService()
        {
            // add json config for DBsettings, token, etc.
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            configuration = builder.Build(); 

            // call connection string from config
            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];
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

        public async Task InsertOrUpdateClassAndLevel(CharacterModel character, Killmail killmail, bool updateKillmail)
        {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                connection.Open();

                var insertLevelSql = 
                @"UPDATE character 
                SET level = @Level
                WHERE name = @CharName
                ";

                await connection.ExecuteAsync(insertLevelSql, new { Level = character.level, CharName = character.name });

                if (updateKillmail)
                {
                    string insertLevelIntoKillmailSql;

                    if (character.isAttacker) 
                    {
                        insertLevelIntoKillmailSql = 
                        @"UPDATE killmail 
                        SET attacker_level = @Level
                        WHERE killmail_raw_id = @KillmailRawId
                        ";
                    }
                    else 
                    {
                        insertLevelIntoKillmailSql = 
                        @"UPDATE killmail 
                        SET victim_level = @Level
                        WHERE killmail_raw_id = @KillmailRawId
                        ";
                    }
                    await connection.ExecuteAsync(insertLevelIntoKillmailSql, new { Level = character.level, KillmailRawId = killmail.killmail_raw_id});
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
                        var insertClassSql = 
                        @"INSERT INTO class (name) 
                        VALUES (@ClassName)
                        ON CONFLICT(name) DO UPDATE
                        SET name = EXCLUDED.name
                        RETURNING id
                        ";

                        var parameters = new DynamicParameters();
                        parameters.Add("@ClassName", character.className);
                        parameters.Add("@ClassId", direction: ParameterDirection.Output);

                        await connection.ExecuteAsync(insertClassSql, parameters);
                        character.classId = parameters.Get<int>("ClassId");
                    }
                }

                // Update character table with class_id
                var insertClassIntoCharSql = 
                @"UPDATE character 
                SET class_id = @ClassId
                WHERE name = @CharName
                ";

                await connection.ExecuteAsync(insertClassIntoCharSql, new { ClassId = character.classId, CharName = character.name });
            }
        }

        private async Task<RawKillMail> InsertRawKillmailAsync(IDbConnection connection, IMessage discordMessage)
        {
            Console.WriteLine(discordMessage.Content.ToString());
            DynamicParameters parameters = new DynamicParameters();
            
            var sql = @"INSERT INTO killmail_raw (discord_message_id, message) Values (@MessageId, @Message)
            ON CONFLICT (discord_message_id) DO UPDATE
            SET message = EXCLUDED.message
            RETURNING id, discord_message_id, message, status_type_id
            "; // insert raw killmail

            decimal messageId = discordMessage.Id;
            parameters.Add("@MessageId", messageId);
            parameters.Add("@Message", discordMessage.Content);

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
            killmailToInsert.victim_id =  await GetOrInsertCharacter(connection, parsedKillmailModel.VictimName, killmailToInsert.victim_guild_id);
            killmailToInsert.attacker_id = await GetOrInsertCharacter(connection, parsedKillmailModel.AttackerName, killmailToInsert.attacker_guild_id);

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
            dynamicParams.Add("@KillMailId", direction: ParameterDirection.Output);

            // Finally, insert killmail 
            var killmailInsertSql = 
            @"INSERT INTO killmail (victim_id, victim_guild_id, attacker_id, attacker_guild_id, zone_id, killed_at, killmail_raw_id)
            VALUES (@victim_id, @victim_guild_id, @attacker_id, @attacker_guild_id, @zone_id, @killed_at, @killmail_raw_id)
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
            killMailInvolvedToInsert.attacker_id = await GetOrInsertCharacter(connection, involved.AttackerName, killMailInvolvedToInsert.attacker_guild_id);
            killMailInvolvedToInsert.melee_damage = involved.MeleeDamage;
            killMailInvolvedToInsert.melee_hits = involved.MeleeHits;
            killMailInvolvedToInsert.spell_damage = involved.SpellDamage;
            killMailInvolvedToInsert.spell_hits = involved.SpellHits;

            var insertQuery = 
            @"INSERT INTO killmail_involved (killmail_id, attacker_id, attacker_guild_id, attacker_level, melee_damage, melee_hits, spell_damage, spell_hits)
            VALUES (@killmail_id, @attacker_id, @attacker_guild_id, @attacker_level, @melee_damage, @melee_hits, @spell_damage, @spell_hits)
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
            var parameters = new DynamicParameters();
            
            var charSelectQuery = @"SELECT id FROM character WHERE name = @Name;";
            var characterId = await connection.ExecuteScalarAsync<int?>(charSelectQuery, new { Name = name});
            if (characterId != null)
            {
                var charUpdateQuery = @"UPDATE character SET guild_id = @GuildId WHERE id = @Id";
                parameters.Add("@Id", characterId.Value);
                parameters.Add("@GuildId", guildId);
                
                await connection.ExecuteAsync(charUpdateQuery, parameters);
                return characterId.Value;
            }
            else
            {
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
    }
}