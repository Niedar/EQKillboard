using System.Text.RegularExpressions;
using EQKillboard.DiscordParser.Models;
using System.Linq;

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
        
        private const string InvolvedPattern = @"(?<attacker>.*)\s*contributed\s*(?:(?<meleeDamage1>\d*) melee damage across (?<meleehit1>\d*) hits?|(?<spellDamage1>\d*) spell damage across (?<spellHit1>\d*) hits?)(?: and\s*(?:(?<meleeDamage2>\d*) melee damage across (?<meleeHit2>\d*) hit?s|(?<spellDamage2>\d*) spell damage across (?<spellHit2>\d*) hit?s)|\.)";
                                                        
        public static ParsedKillMail ParseKillmail(string input) {
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
                            extractedKillmail.Zone = group.Value.Trim();
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
                            ParseInvolved(extractedKillmail, group.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                return null;
            }

            return extractedKillmail;

        }

        private static void ParseInvolved(ParsedKillMail killMail, string involvedText)
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
                            case "meleeDamage1":
                            case "meleeDamage2":
                                if (int.TryParse(group.Value.Trim(), out var meleeDamage))
                                {
                                    parsedInvolved.MeleeDamage = meleeDamage;
                                }
                                break;
                            case "meleeHit1":
                            case "meleeHit2":
                                if (int.TryParse(group.Value.Trim(), out var meleeHits))
                                {
                                    parsedInvolved.MeleeHits = meleeHits;
                                }
                                break;
                            case "spellDamage1":
                            case "spellDamage2":
                                if (int.TryParse(group.Value.Trim(), out var spellDamage))
                                {
                                    parsedInvolved.SpellDamage = spellDamage;
                                }
                                break;
                            case "spellHit1":
                            case "spellHit2":
                                if (int.TryParse(group.Value.Trim(), out var spellHits))
                                {
                                    parsedInvolved.SpellHits = spellHits;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    killMail.Involved.Add(parsedInvolved);
                }
            }
        }
    }
}
