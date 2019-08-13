using System;

namespace EQKillboardDiscordParser.Entities {
    public class Killmail {
        public int id { get; set; }
        public int victim_id { get; set; }
        public int? victim_guild_id { get; set; }
        public int victim_level { get; set; }
        public int attacker_id { get; set; }
        public int? attacker_guild_id { get; set; }
        public int attacker_level { get; set; }
        public int zone_id { get; internal set; }
        public DateTime killed_at { get; set; }
        public int killmail_raw_id { get; set; }
    }
}