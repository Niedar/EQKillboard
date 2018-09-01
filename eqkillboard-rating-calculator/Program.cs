using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Glicko2;
using eqkillboard_rating_calculator.Db;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Data;

namespace eqkillboard_rating_calculator
{
    class Program
    {
        private static string DbConnectionString;
        static void Main(string[] args)
        {

            // add json config for DBsettings
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            var configuration = builder.Build(); 

            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];


            CalculateRatingFromStart();
        }

        public static void CalculateRatingFromStart()
        {
            var calculator = new RatingCalculator();
            var results = new RatingPeriodResults();
            var allChars = new List<CharacterModel>();

            var allKillmails = new List<KillmailModel>();

            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                var query = @"
                WITH hours AS (
                    SELECT generate_series(
                        date_trunc('hour', (SELECT killed_at FROM killmail ORDER BY killed_at LIMIT 1)),
                        date_trunc('hour', (SELECT killed_at FROM killmail ORDER BY killed_at DESC LIMIT 1)),
                        '1 hour'::interval
                    ) AS hour
                )

                SELECT
                    hours.hour,
					killmail.id,
					killmail.killed_at,
                    killmail.victim_id,
                    killmail.attacker_id
                FROM hours
                LEFT JOIN killmail ON date_trunc('hour', killmail.killed_at) = hours.hour
				LEFT JOIN killmail AS prev ON prev.killed_at = (SELECT MAX(killed_at) 
														 FROM killmail AS k1
														 WHERE k1.victim_id = killmail.victim_id
														 AND k1.killed_at < killmail.killed_at
														)
				WHERE prev.killed_at IS NULL OR (killmail.killed_at - prev.killed_at) > make_interval(mins => 10)
				ORDER BY hours.hour, killmail.killed_at
				;  
                ";

                try {
                    allKillmails = connection.Query<KillmailModel>(query)
                                   .ToList();
                }

                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                var query = @"
                SELECT id, name FROM character      
                ";

                try {
                    allChars = connection.Query<CharacterModel>(query).ToList();
                }

                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            // Set initial rating and rd for all characters
            foreach (var character in allChars) {
                character.rating = new Rating(calculator);
            }

            var killmailsGroupedByTimeslice = allKillmails.ToLookup(x => x.Timeslice);

            foreach (var timesliceGroup in killmailsGroupedByTimeslice)
            {
                // Console.WriteLine(timesliceGroup);
                // No games in rating period.
                if (timesliceGroup.Count() == 1 && timesliceGroup.First().victim_id == null)
                {
                    // Perform Rating calculation decay for all characters involved in kills so far
                    calculator.UpdateRatings(results);
                }
                else
                {
                    foreach (var killmailInTimesliceGroup in timesliceGroup)
                    {
                        if(killmailInTimesliceGroup.attacker_id == null) {
                            continue;
                        }

                        // Perform Rating calculation
                        var attacker = allChars.Find(x => x.id == killmailInTimesliceGroup.attacker_id.Value);
                        var victim = allChars.Find(x => x.id == killmailInTimesliceGroup.victim_id.Value);

                        results.AddResult(attacker.rating, victim.rating);

                        // Set updated_at property for later checks on updating rating
                        var timesliceOffset = new TimeSpan(0, 1, 0, 0);;
                        attacker.updated_at = killmailInTimesliceGroup.Timeslice + timesliceOffset;
                        victim.updated_at = killmailInTimesliceGroup.Timeslice + timesliceOffset;
                    }

                    // Perform Rating calculations (includes decay for all characters that are not in rating period if they were involved in kills in earlier timeslices)
                    calculator.UpdateRatings(results);
                }
            }

            foreach (var character in allChars) {
                InsertRating(character);
            }

        }

        private static CharacterModel InsertRating(CharacterModel character) {
            using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                var query = @"INSERT INTO rating (character_id, rating, rd, updated_at)
                VALUES (@character_id, @rating, @rd, @updated_at)
                ";

                var parameters = new DynamicParameters();
                parameters.Add("@character_id", character.id);
                parameters.Add("@rating", character.rating.GetRating());
                parameters.Add("@rd",character.rating.GetRatingDeviation());
                parameters.Add("@updated_at", character.updated_at);

                try {
                    var result = connection.ExecuteScalar(query, parameters);
                }
                catch(Exception ex) {
                    Console.WriteLine(ex);
                }

                return character;
            }

        }
    }

    public class KillmailModel 
    {
        public DateTime Timeslice { get; set; }
        public int? victim_id { get; set; }
        public int? attacker_id { get; set; }
        // public DateTime Timeslice { get; set; }
    }

    public class CharacterModel 
    {
        public int id { get; set; }
        public string name { get; set; }

        public Rating rating { get; set; }
        public DateTime updated_at { get; set; }
    }
}
