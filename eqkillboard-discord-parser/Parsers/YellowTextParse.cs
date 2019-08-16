using System;
using System.Text.RegularExpressions;
using EQKillboard.DiscordParser.Entities;
using EQKillboard.DiscordParser.Models;

namespace EQKillboard.DiscordParser.Parsers {
    public class YellowTextParser {
        private string pattern { get; set; } = @"(?<datetime>\d{4}[-]\d{2}[-]\d{2}\s*\d{2}[:]\d{2}[:]\d{2})"
                                                + @"\s(?<victimName>.*)"
                                                + @"\s[<](?<victimGuild>.*)[>]"
                                                + @"\shas been killed by"
                                                + @"\s(?<attackerName>.*)"
                                                + @"\s[<](?<attackerGuild>.*)[>]"
                                                + @"\sin"
                                                + @"\s(?<zone>.*)!";
                                                        
        public KillMailModel ExtractKillmail(string input) {
            var extractedKillmail = new KillMailModel();
            Match killmailMatch = Regex.Match(input, pattern);
            if (killmailMatch.Success)
            {
                foreach (Group group in killmailMatch.Groups) {
                    switch(group.Name)
                    {
                        case "datetime":
                            extractedKillmail.KilledAt = group.Value;
                            break;
                        case "victimName":
                            extractedKillmail.VictimName = group.Value;
                            break;
                        case "victimGuild":
                            extractedKillmail.VictimGuild = group.Value;
                            break;
                        case "attackerName":
                            extractedKillmail.AttackerName = group.Value;
                            break;
                        case "attackerGuild":
                            extractedKillmail.AttackerGuild = group.Value;
                            break;
                        case "zone":
                            extractedKillmail.Zone = group.Value;
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
    }
}
