namespace DeveHangmanBot
{
    public class WordList
    {
        public WordList(string name, params string[] words)
        {
            Name = name;
            Words = words;
        }

        public string Name { get; }
        public string[] Words { get; }
    }
}
