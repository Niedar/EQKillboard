namespace EQKillboardDiscordParser.Models {
    public class KillmailModel {
        public string victimName { get; set; }
        public string victimGuild { get; set; }
        public string attackerName { get; set; }
        public string attackerGuild { get; set; }
        public string zone { get; set; }
        public int zoneId { get; set; }
        public string killedAt { get; set; }
        public int killmail_raw_id { get; set; }
    }
}