using System;
using System.Collections.Generic;

namespace EQKillboard.DiscordParser.Entities {
    public class Killmail {
        public int id { get; set;}
        public int victim_id { get; set; }
        public int? victim_guild_id { get; set; }
        public int? victim_level { get; set; }
        public int attacker_id { get; set; }
        public int? attacker_guild_id { get; set; }
        public int? attacker_level { get; set; }
        public int zone_id { get; internal set; }
        public DateTime killed_at { get; set; }
        public int killmail_raw_id { get; set; }
        public int? looted_item { get; set;}
        public int? looted_by { get; set;}
        public int season { get; set; }
        public List<KillmailInvolved> Involved { get; set;} = new List<KillmailInvolved>();
    }

    public class KillmailInvolved
    {
        public int killmail_id { get; set; }
        public int attacker_id { get; set; }
        public int? attacker_guild_id { get; set; }
        public int? attacker_level { get; set; }
        public int? melee_damage { get; set; }
        public int? melee_hits { get; set; }
        public int? spell_damage { get; set; }
        public int? spell_hits { get; set; }
        public int? dispel_slots {get; set;}
    }
}