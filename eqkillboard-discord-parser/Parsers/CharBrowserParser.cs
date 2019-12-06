using System.Text.RegularExpressions;

public static class CharBrowserParser
{
        private const string ClassLevelPattern = @"(?<level>\d*)"
                                                + @"\s"
                                                + @"(?<className>\w*)"
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
        public static string ParseClass(string classLevel) {
            string className = "";
            Match classLevelMatch = Regex.Match(classLevel, ClassLevelPattern);
            foreach(Group group in classLevelMatch.Groups) {
                if (group.Name == "className") {
                    className = group.Value;
                }
            }

            return className;
        }
}