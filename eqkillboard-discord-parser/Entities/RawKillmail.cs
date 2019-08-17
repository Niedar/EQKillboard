namespace EQKillboard.DiscordParser.Entities
{
    public class RawKillMail
    {
        public int id { get; set; }
        public ulong discord_message_id { get; set; }
        public string message { get; set; }
        public int status_type_id { get; set;}
    }
}