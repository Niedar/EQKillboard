namespace eqkillboard_discord_parser.Models {
    public class CharacterModel {
        public string name { get; set; }
        public int? classId { get; set; }
        public string className { get; set; }
        public int? level { get; set; }
        public string classLevel { get; set; }
        public bool isAttacker { get; set; }
    }
}