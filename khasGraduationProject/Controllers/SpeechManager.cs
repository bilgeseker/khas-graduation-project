using khasGraduationProject.Models;
using System.Speech.Recognition;
using System.Text;
using System.Text.RegularExpressions;

namespace khasGraduationProject.Controllers
{
    public static class SpeechManager
    {
        public static string AudioToText(string filePath)
        {
            RecognizerInfo info = null;
            foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (ri.Culture.TwoLetterISOLanguageName.Equals("en"))
                {
                    info = ri;
                    break;
                }
            }

            SpeechRecognitionEngine sre = new SpeechRecognitionEngine(info);

            var context = new WebContext();
            var audiosList = context.audios.ToList();

            List<string> list = new List<string>();

            foreach (var audio in audiosList)
            {
                string[] words = audio.similarText.Split(',');
                foreach (var word in words)
                {
                    if (!String.IsNullOrEmpty(word)) {
                        list.Add(word.TrimStart().TrimEnd());
                    }
                }
            }

            string[] grammarText = list.ToArray();
            Choices choices = new Choices(grammarText);
            GrammarBuilder grammarBuilder = new GrammarBuilder(choices);
            grammarBuilder.Culture = new System.Globalization.CultureInfo("en-US");
            Grammar grammar = new Grammar(grammarBuilder);
            sre.LoadGrammar(grammar);

            sre.SetInputToWaveFile(filePath);
            sre.BabbleTimeout = new TimeSpan(Int32.MaxValue);
            sre.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue);
            sre.EndSilenceTimeout = new TimeSpan(100000000);
            sre.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000);

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                try
                {
                    var recText = sre.Recognize();
                    if (recText == null)
                        break;

                    sb.Append(recText.Text);
                }
                catch (Exception ex)
                {
                    break;
                }
                finally
                {
                    sre.UnloadAllGrammars();
                }
            }

            if (sb.Length <= 0)
            {
                sb.Clear();

                sre = new SpeechRecognitionEngine(info);
                sre.SetInputToWaveFile(filePath);
                sre.BabbleTimeout = new TimeSpan(Int32.MaxValue);
                sre.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue);
                sre.EndSilenceTimeout = new TimeSpan(100000000);
                sre.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000);

                Grammar gr = new DictationGrammar();              
                sre.LoadGrammar(gr);

                while (true)
                {
                    try
                    {
                        var recText = sre.Recognize();
                        if (recText == null)
                            break;

                        sb.Append(recText.Text);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                    finally
                    {
                        sre.UnloadAllGrammars();
                    }
                }
            }

            return sb.ToString();
        }

        // Compares the two strings based on letter pair matches
        public static double CompareStrings(string str1, string str2)
        {
            List<string> pairs1 = WordLetterPairs(str1.ToUpper());
            List<string> pairs2 = WordLetterPairs(str2.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                for (int j = 0; j < pairs2.Count; j++)
                {
                    if (pairs1[i] == pairs2[j])
                    {
                        intersection++;
                        pairs2.RemoveAt(j); // Must remove the match to prevent "AAAA" from appearing to match "AA" with 100% success
                        break;
                    }
                }
            }

            return (2.0 * intersection * 100) / union; //returns in percentage
                                                       //return (2.0 * intersection) / union; //returns in score from 0 to 1
        }

        // Gets all letter pairs for each
        private static List<string> WordLetterPairs(string str)
        {
            List<string> AllPairs = new List<string>();

            // Tokenize the string and put the tokens/words into an array
            string[] Words = Regex.Split(str, @"\s");

            // For each word
            for (int w = 0; w < Words.Length; w++)
            {
                if (!string.IsNullOrEmpty(Words[w]))
                {
                    // Find the pairs of characters
                    String[] PairsInWord = LetterPairs(Words[w]);

                    for (int p = 0; p < PairsInWord.Length; p++)
                    {
                        AllPairs.Add(PairsInWord[p]);
                    }
                }
            }
            return AllPairs;
        }

        // Generates an array containing every two consecutive letters in the input string
        private static string[] LetterPairs(string str)
        {
            int numPairs = str.Length - 1;
            string[] pairs = new string[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                pairs[i] = str.Substring(i, 2);
            }
            return pairs;
        }
    }
}
