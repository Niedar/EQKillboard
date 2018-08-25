using System;
using System.Collections.Generic;
using System.Linq;
using Glicko2;

namespace eqkillboard_rating_calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Instantiate a RatingCalculator object.
            // At instantiation, you can set the default rating for a player's volatility and
            // the system constant for your game ("τ", which constrains changes in volatility
            // over time) or just accept the defaults.
            var calculator = new RatingCalculator(/* initVolatility, tau */);

            // Instantiate a Rating object for each player.
            var player1 = new Rating(calculator/* , rating, ratingDeviation, volatility */);
            var player2 = new Rating(calculator/* , rating, ratingDeviation, volatility */);
            var player3 = new Rating(calculator/* , rating, ratingDeviation, volatility */);

            // Instantiate a RatingPeriodResults object.
            var results = new RatingPeriodResults();

            // Add game results to the RatingPeriodResults object until you reach the end of your rating period.
            // Use addResult(winner, loser) for games that had an outcome.
            results.AddResult(player1, player2);
            // Use addDraw(player1, player2) for games that resulted in a draw.
            results.AddDraw(player1, player2);
            // Use addParticipant(player) to add players that played no games in the rating period.
            results.AddParticipant(player3);

            // Once you've reached the end of your rating period, call the updateRatings method
            // against the RatingCalculator; this takes the RatingPeriodResults object as argument.
            //  * Note that the RatingPeriodResults object is cleared down of game results once
            //    the new ratings have been calculated.
            //  * Participants remain within the RatingPeriodResults object, however, and will
            //    have their rating deviations recalculated at the end of future rating periods
            //    even if they don't play any games. This is in-line with Glickman's algorithm.
            calculator.UpdateRatings(results);

            // Access the getRating, getRatingDeviation, and getVolatility methods of each
            // player's Rating to see the new values.
            var players = new[] {player1, player2, player3};
            for (var index = 0; index < players.Length; index++)
            {
                var player = players[index];
                Console.WriteLine("Player #" + index + " values: " + player.GetRating() + ", " +
                    player.GetRatingDeviation() + ", " + player.GetVolatility());
            }
        }

        public static void CalculateRatingFromStart()
        {
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
				killmail.victim_id,
				killmail.attacker_id
            FROM hours
            LEFT JOIN killmail ON date_trunc('hour', killmail.killed_at) = hours.hour;       
            ";

            var allCharacterIds = new HashSet<int>();
            var allKillmails = new List<KillmailModel>();
            var killmailsGroupedByTimeslice = allKillmails.ToLookup(x => x.Timeslice);

            foreach (var timesliceGroup in killmailsGroupedByTimeslice)
            {
                // No games in rating period.
                if (timesliceGroup.Count() == 1 && timesliceGroup.First().Victim_Id == null)
                {
                    // Perform Rating calculation decary for all characters
                }
                else
                {
                    var characterIdsInRatingPeriod = new HashSet<int>();
                    foreach (var killmailInTimesliceGroup in timesliceGroup)
                    {
                        characterIdsInRatingPeriod.Add(killmailInTimesliceGroup.Attacker_Id.Value);
                        characterIdsInRatingPeriod.Add(killmailInTimesliceGroup.Victim_Id.Value);

                        // Perform Rating calculation
                    }

                    // Perform Rating calculation decary for all charaters that are not in rating period
                    var charaterIdsNotInRatingPeriod = allCharacterIds.Except(characterIdsInRatingPeriod);
                }

            }
        }
    }

    public class KillmailModel 
    {
        public DateTime Timeslice { get; set; }
        public int? Victim_Id { get; set; }
        public int? Attacker_Id { get; set; }
    }
}
