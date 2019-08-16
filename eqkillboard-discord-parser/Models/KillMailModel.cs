namespace EQKillboard.DiscordParser.Models {
    public class KillMailModel {
        public string VictimName { get; set; }
        public string VictimGuild { get; set; }
        public string AttackerName { get; set; }
        public string AttackerGuild { get; set; }
        public string Zone { get; set; }
        public string KilledAt { get; set; }
        public int KillMailRawId { get; set; }
    }

    public class KillMailInvolvedModel {
        public string AttackerName { get; set; }
        public string AttackerGuild { get; set; }
        public int? AttackerLevel { get; set; }
        public int? MeleeDamage { get; set; }
        public int? MeleeHits { get; set; }
        public int? SpellDamage { get; set; }
        public int? SpellHits { get; set; }
    }
}