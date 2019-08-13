using System;
using System.Text.RegularExpressions;
using EQKillboardDiscordParser.Entities;
using EQKillboardDiscordParser.Models;

namespace EQKillboardDiscordParser {
    public class KillMailParser {
        private string pattern { get; set; } = @"(?<datetime>\d{4}[-]\d{2}[-]\d{2}\s*\d{2}[:]\d{2}[:]\d{2})"
                                                + @"\s(?<victimName>.*)"
                                                + @"\s[<](?<victimGuild>.*)[>]"
                                                + @"\shas been killed by"
                                                + @"\s(?<attackerName>.*)"
                                                + @"\s[<](?<attackerGuild>.*)[>]"
                                                + @"\sin"
                                                + @"\s(?<zone>.*)!";
        
        public string classLevelPattern { get; set; } = @"(?<level>\d*)"
                                                        + @"\s"
                                                        + @"(?<charName>\w*)"
                                                        + @"[A-Z]";
                                                        
        public KillmailModel ExtractKillmail(string input) {
            var extractedKillmail = new KillmailModel();
            Match killmailMatch = Regex.Match(input, pattern);
            if (killmailMatch.Success)
            {
                foreach (Group group in killmailMatch.Groups) {
                    switch(group.Name)
                    {
                        case "datetime":
                            extractedKillmail.killedAt = group.Value;
                            break;
                        case "victimName":
                            extractedKillmail.victimName = group.Value;
                            break;
                        case "victimGuild":
                            extractedKillmail.victimGuild = group.Value;
                            break;
                        case "attackerName":
                            extractedKillmail.attackerName = group.Value;
                            break;
                        case "attackerGuild":
                            extractedKillmail.attackerGuild = group.Value;
                            break;
                        case "zone":
                            extractedKillmail.zone = group.Value;
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
        
        public string ExtractLevel(string classLevel) {
            string level = "";
            Match classLevelMatch = Regex.Match(classLevel, classLevelPattern);
            foreach(Group group in classLevelMatch.Groups) {
                if (group.Name == "level") {
                    level = group.Value;
                }
            }

            return level;
        }

        public string ExtractChar(string classLevel) {
            string charName = "";
            Match classLevelMatch = Regex.Match(classLevel, classLevelPattern);
            foreach(Group group in classLevelMatch.Groups) {
                if (group.Name == "charName") {
                    charName = group.Value;
                }
            }

            return charName;
        }
    }
}
