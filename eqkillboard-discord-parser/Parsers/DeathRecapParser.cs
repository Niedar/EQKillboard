using System.Text.RegularExpressions;
using EQKillboard.DiscordParser.Models;
using System.Linq;
using EQKillboard.DiscordParser.Scrapers;
using System.Threading.Tasks;

namespace EQKillboard.DiscordParser.Parsers {
    public static class DeathRecapParser {
        private const string Pattern = @"(?<datetime>\d{4}[-]\d{2}[-]\d{2}\s*\d{2}[:]\d{2}[:]\d{2})"
                                                + @"\s*(?<victimName>.*)"
                                                + @"\skilled by"
                                                + @"\s(?<attackerName>.*)"
                                                + @"\sin"
                                                + @"\s(?<zone>.*)"
                                                + @"\s*Killing blow:"
                                                + @"\s(?<killingBlow>.*)"
                                                + @"\s*Overdamage:"
                                                + @"\s(?<overDamage>.*)"
                                                + @"\s*(?<involvedText>(?:.|\n)*)";
        
        // private const string InvolvedPattern = @"(?<attacker>.*)\s*contributed\s*(?:(?<meleeDamage1>\d*) melee damage across (?<meleeHit1>\d*) hits?|(?<spellDamage1>\d*) spell damage across (?<spellHit1>\d*) hits?)(?: and\s*(?:(?<meleeDamage2>\d*) melee damage across (?<meleeHit2>\d*) hits?|(?<spellDamage2>\d*) spell damage across (?<spellHit2>\d*) hits?)|\.)";
        private const string InvolvedPattern = @"(?<attacker>.*)\s*contributed\s(?<contribution>\s*.*).";
        private const string MeleeDamagePattern = @"(?<meleeDamage>\d*) melee damage across (?<meleeHit>\d*) hits?";
        private const string SpellDamagePattern = @"(?<spellDamage>\d*) spell damage across (?<spellHit>\d*) hits?";
        private const string DispelPattern = @"(?<dispelSlots>\d*) slots? of dispels?";
                                                        
        public static async Task<ParsedKillMail> ParseKillmail(string input) {
            // Remove special formatting
            var lines = input.Split(new [] { '\r', '\n' });
            if (lines.Any() && lines.FirstOrDefault().Contains("```"))
            {
                input = string.Join("\n", lines.Skip(1));
            }
            input = input.Replace("**", "");

            var extractedKillmail = new ParsedKillMail();
            Match killmailMatch = Regex.Match(input, Pattern);
            if (killmailMatch.Success)
            {
                foreach (Group group in killmailMatch.Groups.Where(x => x.Success)) {
                    switch(group.Name)
                    {
                        case "datetime":
                            extractedKillmail.KilledAt = group.Value.Trim();
                            break;
                        case "victimName":
                            extractedKillmail.VictimName = group.Value.Trim();
                            break;
                        case "attackerName":
                            extractedKillmail.AttackerName = group.Value.Trim();
                            break;
                        case "zone":
                            extractedKillmail.Zone = ZoneMapper.fullZoneName(group.Value.Trim());
                            break;
                        case "killingBlow":
                            if (int.TryParse(group.Value.Trim(), out var killingBlow))
                                extractedKillmail.KillingBlow = killingBlow;
                            break;
                        case "overDamage":
                            if (int.TryParse(group.Value.Trim(), out var overDamage))
                                extractedKillmail.OverDamage = overDamage;
                            break;
                        case "involvedText":
                            await ParseInvolved(extractedKillmail, group.Value);
                            break;
                        default:
                            break;
                    }
                }

                
                // Get level and class and guild for each char
                var victimScraper = new CharBrowserScraper(extractedKillmail.VictimName);
                var attackerScraper = new CharBrowserScraper(extractedKillmail.AttackerName);
                await victimScraper.Fetch();
                await attackerScraper.Fetch();

                extractedKillmail.AttackerGuild = attackerScraper.Guild;
                extractedKillmail.AttackerLevel = attackerScraper.Level;
                extractedKillmail.AttackerClass = attackerScraper.Class;
                extractedKillmail.AttackerIsNpc = attackerScraper.IsNpc;
                extractedKillmail.VictimGuild = victimScraper.Guild;
                extractedKillmail.VictimLevel = victimScraper.Level;
                extractedKillmail.VictimClass = victimScraper.Class;
                extractedKillmail.VictimIsNpc = victimScraper.IsNpc;
            }
            else
            {
                return null;
            }

            return extractedKillmail;

        }

        private static async Task ParseInvolved(ParsedKillMail killMail, string involvedText)
        {
            var involvedMatches = Regex.Matches(involvedText, InvolvedPattern);
            foreach(Match involvedMatch in involvedMatches)
            {
                if (involvedMatch.Success)
                {
                    var parsedInvolved = new ParsedKillMailInvolved();
                    foreach (Group group in involvedMatch.Groups.Where(x => x.Success)) {
                        switch(group.Name)
                        {
                            case "attacker":
                                parsedInvolved.AttackerName = group.Value.Trim();
                                break;
                            case "contribution":
                                var contributionText = group.Value.Trim();
                                var meleeDamageMatch = Regex.Match(contributionText, MeleeDamagePattern)?.Groups.FirstOrDefault(x => x.Name == "meleeDamage");
                                var meleeHitMatch = Regex.Match(contributionText, MeleeDamagePattern)?.Groups.FirstOrDefault(x => x.Name == "meleeHit");
                                var spellDamageMatch = Regex.Match(contributionText, SpellDamagePattern)?.Groups.FirstOrDefault(x => x.Name == "spellDamage");
                                var spellHitMatch = Regex.Match(contributionText, SpellDamagePattern)?.Groups.FirstOrDefault(x => x.Name == "spellHit");
                                var dispelMatch = Regex.Match(contributionText, DispelPattern)?.Groups.FirstOrDefault(x => x.Name == "dispelSlots");
                                
                                if (meleeDamageMatch != null && int.TryParse(meleeDamageMatch.Value.Trim(), out var meleeDamage))
                                {
                                    parsedInvolved.MeleeDamage = meleeDamage;
                                }
                                if (meleeHitMatch != null && int.TryParse(meleeHitMatch.Value.Trim(), out var meleeHit))
                                {
                                    parsedInvolved.MeleeHits = meleeHit;
                                }
                                if (spellDamageMatch != null && int.TryParse(spellDamageMatch.Value.Trim(), out var spellDamage))
                                {
                                    parsedInvolved.SpellDamage = spellDamage;
                                }
                                if (spellHitMatch != null && int.TryParse(spellHitMatch.Value.Trim(), out var spellHit))
                                {
                                    parsedInvolved.SpellHits = spellHit;
                                }
                                if (dispelMatch != null && int.TryParse(dispelMatch.Value.Trim(), out var dispelSlots))
                                {
                                    parsedInvolved.DispelSlots = dispelSlots;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    // Get level and class and guild for each char
                    var attackerScraper = new CharBrowserScraper(parsedInvolved.AttackerName);
                    await attackerScraper.Fetch();

                    parsedInvolved.AttackerGuild = attackerScraper.Guild;
                    parsedInvolved.AttackerLevel = attackerScraper.Level;
                    parsedInvolved.AttackerClass = attackerScraper.Class;
                    parsedInvolved.AttackerIsNpc = attackerScraper.IsNpc;

                    killMail.Involved.Add(parsedInvolved);
                }
            }
        }
    }
}
