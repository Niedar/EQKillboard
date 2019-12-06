using System.Collections.Generic;
namespace EQKillboard.DiscordParser.Models {
    public class ParsedKillMail {
        public string VictimName { get; set; }
        public string VictimGuild { get; set; }
        public int? VictimLevel { get; set; }
        public string VictimClass { get; set; }
        public bool VictimIsNpc { get; set; }
        public string AttackerName { get; set; }
        public string AttackerGuild { get; set; }
        public int? AttackerLevel { get; set; }
        public string AttackerClass { get; set; }
        public bool AttackerIsNpc { get; set; }
        public string Zone { get; set; }
        public int? KillingBlow { get; set; }
        public int? OverDamage { get; set; }
        public string KilledAt { get; set; }
        public int KillMailRawId { get; set; }
        public List<ParsedKillMailInvolved> Involved { get; set; } = new List<ParsedKillMailInvolved>();
    }

    public class ParsedKillMailInvolved {
        public string AttackerName { get; set; }
        public string AttackerGuild { get; set;}
        public int? AttackerLevel { get; set; }
        public string AttackerClass { get; set; }
        public bool AttackerIsNpc { get; set; }
        public int? MeleeDamage { get; set; }
        public int? MeleeHits { get; set; }
        public int? SpellDamage { get; set; }
        public int? SpellHits { get; set; }
    }
}