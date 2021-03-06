using System.Collections.Generic;

public static class ZoneMapper {

    public static string fullZoneName(string shortZoneName)
    {
        if (zoneDictionary.ContainsKey(shortZoneName))
        {
            return zoneDictionary[shortZoneName];
        }
        else
        {
            return "Unknown";
        }
    }


    private static Dictionary<string, string> zoneDictionary = new Dictionary<string, string>()
    {
        {"tox", "Toxxulia Forest"},
        {"feerrott", "The Feerrott"},
        {"innothule", "Innothule Swamp"},
        {"nro", "Northern Desert of Ro"},
        {"nektulos", "The Nektulos Forest"},
        {"blackburrow", "Blackburrow"},
        {"sro", "Southern Desert of Ro"},
        {"oasis", "Oasis of Marr"},
        {"befallen", "Befallen"},
        {"warrens", "The Warrens"},
        {"erudsxing", "Erud's Crossing"},
        {"misty", "Misty Thicket"},
        {"crushbone", "Crushbone"},
        {"cauldron", "Dagnor's Cauldron"},
        {"lavastorm", "Lavastorm Mountains"},
        {"kurn", "Kurn's Tower"},
        {"everfrost", "Everfrost"},
        {"qeytoqrg", "The Qeynos Hills"},
        {"paludal", "The Paludal Caverns"},
        {"steamfont", "Steamfont Mountains"},
        {"runnyeye", "The Liberated Citadel of Runnyeye"},
        {"kerraridge", "Kerra Isle"},
        {"guktop", "Guk"},
        {"qey2hh1", "The Western Plains of Karana"},
        {"southkarana", "The Southern Plains of Karana"},
        {"shadeweaver", "Shadeweaver's Thicket"},
        {"najena", "Najena"},
        {"lakerathe", "Lake Rathetear"},
        {"lfaydark", "The Lesser Faydark"},
        {"fieldofbone", "Field of Bone"},
        {"swampofnohope", "The Swamp of No Hope"},
        {"stonebrunt", "The Stonebrunt Mountains"},
        {"unrest", "The Estate of Unrest"},
        {"rathemtn", "The Rathe Mountains"},
        {"oot", "Ocean of Tears"},
        {"netherbian", "Netherbian Lair"},
        {"northkarana", "The Northern Plains of Karana"},
        {"hollowshade", "Hollowshade Moor"},
        {"commons", "West Commonlands"},
        {"lakeofillomen", "Lake of Ill Omen"},
        {"ecommons", "East Commonlands"},
        {"mseru", "Marus Seru"},
        {"eastkarana", "Eastern Plains of Karana"},
        {"qcat", "The Qeynos Aqueduct System"},
        {"kithicor", "Kithicor Forest"},
        {"dalnir", "The Crypt of Dalnir"},
        {"warslikswood", "The Warsliks Woods"},
        {"soldunga", "Solusek's Eye"},
        {"highpass", "Highpass Hold"},
        {"butcher", "Butcherblock Mountains"},
        {"cazicthule", "Lost Temple of Cazic-Thule"},
        {"permafrost", "The Permafrost Caverns"},
        {"mistmoore", "The Castle of Mistmoore"},
        {"frontiermtns", "Frontier Mountains"},
        {"iceclad", "The Iceclad Ocean"},
        {"highkeep", "High Keep"},
        {"droga", "Temple of Droga"},
        {"frozenshadow", "The Tower of Frozen Shadow"},
        {"qeynos2", "North Qeynos"},
        {"nurga", "The Mines of Nurga"},
        {"paw", "The Lair of the Splitpaw"},
        {"crystal", "The Crystal Caverns"},
        {"kaesora", "Kaesora"},
        {"timorous", "Timorous Deep"},
        {"soltemple", "The Temple of Solusek Ro"},
        {"dreadlands", "Dreadlands"},
        {"emeraldjungle", "The Emerald Jungle"},
        {"twilight", "The Twilight Sea"},
        {"qeynos", "South Qeynos"},
        {"grimling", "Grimling Forest"},
        {"jaggedpine", "The Jaggedpine Forest"},
        {"thurgadina", "The City of Thurgadin"},
        {"eastwastes", "Eastern Wastes"},
        {"citymist", "The City of Mist"},
        {"gukbottom", "Ruins of Old Guk"},
        {"rivervale", "Rivervale"},
        {"dawnshroud", "The Dawnshroud Peaks"},
        {"scarlet", "The Scarlet Desert"},
        {"erudnext", "Erudin"},
        {"tenebrous", "The Tenebrous Mountains"},
        {"letalis", "Mons Letalis"},
        {"echo", "The Echo Caverns"},
        {"gfaydark", "Greater Faydark"},
        {"cobaltscar", "Cobaltscar"},
        {"burningwood", "The Burning Wood"},
        {"greatdivide", "The Great Divide"},
        {"soldungb", "Nagafen's Lair"},
        {"freporte", "East Freeport"},
        {"paineel", "Paineel"},
        {"erudnint", "Erudin Palace"},
        {"firiona", "Firiona Vie"},
        {"akanon", "Ak'Anon"},
        {"sharvahl", "The City of Shar Vahl"},
        {"overthere", "The Overthere"},
        {"kedge", "Kedge Keep"},
        {"neriaka", "Neriak - Foreign Quarter"},
        {"felwithea", "Northern Felwithe"},
        {"wakening", "The Wakening Land"},
        {"felwitheb", "Southern Felwithe"},
        {"thegrey", "The Grey"},
        {"trakanon", "Trakanon's Teeth"},
        {"bazaar", "The Bazaar"},
        {"freportw", "West Freeport"},
        {"griegsend", "Grieg's End"},
        {"katta", "Katta Castellum"},
        {"qrg", "The Surefall Glade"},
        {"cabeast", "Cabilis East"},
        {"grobb", "Grobb"},
        {"kaladima", "South Kaladim"},
        {"skyshrine", "Skyshrine"},
        {"skyfire", "The Skyfire Mountains"},
        {"freportn", "North Freeport"},
        {"oggok", "Oggok"},
        {"karnor", "Karnor's Castle"},
        {"kael", "Kael Drakkel"},
        {"kaladimb", "North Kaladim"},
        {"cabwest", "Cabilis West"},
        {"neriakb", "Neriak - Commons"},
        {"fearplane", "Plane of Fear"},
        {"pojustice", "The Plane of Justice"},
        {"chardok", "Chardok"},
        {"neriakc", "Neriak - 3rd Gate"},
        {"shadowhaven", "Shadow Haven"},
        {"charasis", "The Howling Stones"},
        {"hole", "The Hole"},
        {"halas", "Halas"},
        {"necropolis", "Dragon Necropolis"},
        {"acrylia", "The Acrylia Caverns"},
        {"potranquility", "The Plane of Tranquility"},
        {"velketor", "Velketor's Labyrinth"},
        {"thedeep", "The Deep"},
        {"thurgadinb", "Icewell Keep"},
        {"fungusgrove", "The Fungus Grove"},
        {"sebilis", "The Ruins of Sebilis"},
        {"akheva", "The Akheva Ruins"},
        {"maiden", "The Maiden's Eye"},
        {"nexus", "Nexus"},
        {"sirens", "Siren's Grotto"},
        {"poinnovation", "The Plane of Innovation"},
        {"sseru", "Sanctus Seru"},
        {"mischiefplane", "The Plane of Mischief"},
        {"hateplane", "Plane of Hate"},
        {"podisease", "The Plane of Disease"},
        {"ssratemple", "Ssraeshza Temple"},
        {"potactics", "Drunder, the Fortress of Zek"},
        {"umbral", "The Umbral Plains"},
        {"westwastes", "The Western Wastes"},
        {"airplane", "The Plane of Sky"},
        {"cshome", "Sunset Home"},
        {"ponightmare", "The Plane of Nightmares"},
        {"growthplane", "The Plane of Growth"},
        {"postorms", "The Plane of Storms"},
        {"poknowledge", "The Plane of Knowledge"},
        {"potorment", "Torment, the Plane of Pain"},
        {"vexthal", "Vex Thal"},
        {"codecay", "The Crypt of Decay"},
        {"poearthb", "The Plane of Earth"},
        {"potimea", "The Plane of Time"},
        {"templeveeshan", "The Temple of Veeshan"},
        {"veeshan", "Veeshan's Peak"},
        {"poeartha", "The Plane of Earth"},
        {"bothunder", "Bastion of Thunder"},
        {"hohonora", "The Halls of Honor"},
        {"poair", "The Plane of Air"},
        {"powater", "The Plane of Water"},
        {"povalor", "The Plane of Valor"},
        {"solrotower", "The Tower of Solusek Ro"},
        {"hohonorb", "The Temple of Marr"},
        {"pofire", "The Plane of Fire"},
        {"sleeper", "The Sleeper's Tomb"},
        {"beholder", "Gorge of King Xorbb"},
        {"nightmareb", "The Lair of Terris Thule"}
    };
}