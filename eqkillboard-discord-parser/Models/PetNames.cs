using System.Collections.Generic;

public static class PetNames {
    public static HashSet<string> Names { get; private set; } = new HashSet<string>();
    static PetNames()
    {

        var part1 = new string[] { "G", "J", "K", "L", "V", "X", "Z" };
        var part2 = new string[] { string.Empty, "ab", "ar", "as", "eb", "en", "ib", "ob", "on" };
	    var part3 = new string[] { string.Empty, "an", "ar", "ek", "ob" };
	    var part4 = new string[] { "er", "ab", "n", "tik" };

        foreach(var p1 in part1)
        {
            foreach(var p2 in part2)
            {
                foreach(var p3 in part3)
                {
                    foreach(var p4 in part4)
                    {
                        var petName = p1 + p2 + p3 + p4;
                        if (petName == "Laser" || petName.Length <= 3 || petName.EndsWith("ektik"))
                            continue;

                        if (!Names.Contains(petName.ToLower()))
                        {
                            Names.Add(petName.ToLower());
                        }
                    }
                }
            }
        }
    }
}