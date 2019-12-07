using System.Collections.Generic;

public static class NpcNames {
    public static HashSet<string> Names { get; private set; } = new HashSet<string>();
    static NpcNames()
    {
        string[] lines = System.IO.File.ReadAllLines(@"npcnames.txt");
        foreach (string line in lines)
        {
            // Use a tab to indent each line of the file.
            if (!Names.Contains(line.Trim().ToLower()))
            {
                Names.Add(line.Trim().ToLower());
            }
        }

    }
}