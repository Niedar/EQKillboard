namespace EQKillboard.DiscordParser.Models {
    public class CharacterModel {
        public string name { get; set; }
        public int? classId { get; set; }
        public string className { get; set; }
        public int? level { get; set; }
        public bool isAttacker { get; set; }
    }
}