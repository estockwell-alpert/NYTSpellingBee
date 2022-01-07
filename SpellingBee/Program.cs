using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpellingBee
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the letters for the spelling bee");
            var letters = Console.ReadLine().ToLower().ToCharArray();

            Console.WriteLine("Enter the Primary letter (the letter that must appear in every word)");
            var primaryLetter = Console.ReadLine().ToLower();

            List<string> Words = new List<String>();

            foreach (var letter in letters)
            {
                var request = (HttpWebRequest)WebRequest.Create(String.Format("https://fly.wordfinderapi.com/api/search?starts_with={0}&dictionary=wwf2&word_sorting=points&group_by_length=true&page_size=100000", letter));

                request.ContentType = "application/json; charset=utf-8";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    var data = reader.ReadToEnd();

                    var results = JsonConvert.DeserializeObject<RequestItem>(data);

                    foreach (var page in results.word_pages)
                    {
                        var words = page.word_list.Select(x => x.word).Where(x => IsMatch(x, letters, primaryLetter));
                        Words.AddRange(words);
                    }
                }
            }

            foreach (var word in Words.OrderBy(x => x))
            {
                if (IsPangram(word, letters))
                {
                    Console.WriteLine(word + " (PANGRAM!)");
                }
                else
                {
                    Console.WriteLine(word);
                }
            }

            Console.ReadLine();
        }

        public static bool IsPangram(string word, char[] letters)
        {
            foreach (var letter in letters)
            {
                if (!word.Contains(letter))
                    return false;
            }

            return true;
        }

        public static bool IsMatch(string word, char[] letters, string primaryLetter)
        {
            if (word.Length < 4) return false;
            word = word.ToLower();

            if (!word.Contains(primaryLetter)) return false;

            foreach (var letter in word)
            {
                if (!letters.Contains(letter))
                    return false;
            }

            return true;
        }
    }

    public class DictionaryResponse
    {
        public RequestItem request { get; set; }
    }

    public class RequestItem
    {
        public int filter_duration { get; set; }
        public int filter_results {get;set;}
        public string letters_for_search { get; set; }
        public IEnumerable<WordPage> word_pages { get; set; }
    }

    public class WordPage
    {
        public IEnumerable<Word> word_list { get; set; }
    }

    public class Word
    {
        public string word { get; set; }
        public int points { get; set; }
    }
}
