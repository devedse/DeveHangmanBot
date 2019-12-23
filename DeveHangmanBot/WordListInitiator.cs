using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DeveHangmanBot
{
    public static class WordListInitiator
    {
        public static List<WordList> WordLists => WordListsLazy.Value;
        private static Lazy<List<WordList>> WordListsLazy { get; } = new Lazy<List<WordList>>(() => GenerateWordLists());

        public static IEnumerable<WordList> GetThese(IEnumerable<string> wordListNumbers)
        {
            foreach (var potentialNumber in wordListNumbers)
            {
                if (int.TryParse(potentialNumber, out int nr))
                {
                    if (nr >= 0 && nr < WordLists.Count)
                    {
                        yield return WordLists[nr];
                    }
                }
            }
        }

        private static List<WordList> GenerateWordLists()
        {
            var wordlists = new List<WordList>();
            wordlists.Add(Instantiate("Pokemon1-151.txt"));
            wordlists.Add(Instantiate("LeagueChampions.txt"));
            wordlists.Add(Instantiate("RetardedWords.txt"));
            return wordlists;
        }

        private static WordList Instantiate(string fileName)
        {
            var wordListsPath = Path.Combine(AssemblyDirectory.Value, "WordLists", fileName);
            var wordListName = Path.GetFileNameWithoutExtension(fileName);

            var words = File.ReadAllLines(wordListsPath).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.ToLowerInvariant().Trim()).Distinct();

            return new WordList(wordListName, words.ToArray());
        }

        private static string CreateLocationOfImageProcessorAssemblyDirectory()
        {
            var assembly = typeof(WordListInitiator).GetTypeInfo().Assembly;
            var assemblyDir = Path.GetDirectoryName(assembly.Location);
            return assemblyDir;
        }


        private static Lazy<string> AssemblyDirectory { get; } = new Lazy<string>(() => CreateLocationOfImageProcessorAssemblyDirectory());
    }
}
