namespace Editor
{
    public class CharacterDecorationInfo
    {
        public CharacterDecorationType DecorationType { get; set; }
        public Position Position { get; set; }
        public string UnicodeString { get; set; } //Only if DecorationType == CharacterDecorationType.Unicode
        public int Index { get; set; } //Should be -1 when not appplicable or invalid
    }
}
