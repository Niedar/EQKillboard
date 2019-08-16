using System.Text.RegularExpressions;

public static class CharBrowserParser
{
        private const string ClassLevelPattern = @"(?<level>\d*)"
                                                + @"\s"
                                                + @"(?<charName>\w*)"
                                                + @"[A-Z]";
        public static string ParseLevel(string classLevel) {
            string level = "";
            Match classLevelMatch = Regex.Match(classLevel, ClassLevelPattern);
            foreach(Group group in classLevelMatch.Groups) {
                if (group.Name == "level") {
                    level = group.Value;
                }
            }

            return level;
        }
        public static string ParseChar(string classLevel) {
            string charName = "";
            Match classLevelMatch = Regex.Match(classLevel, ClassLevelPattern);
            foreach(Group group in classLevelMatch.Groups) {
                if (group.Name == "charName") {
                    charName = group.Value;
                }
            }

            return charName;
        }
}