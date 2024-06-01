using khasGraduationProject.Models;
using System.Speech.Recognition;
using System.Text;
using System.Text.RegularExpressions;

namespace khasGraduationProject.Controllers
{
    public static class SpeechManager
    {
        public static string AudioToText(string filePath, int audio_id)
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
            var audiosList = context.audios.Where(audio => audio.id == audio_id).ToList();

            List<string> list = new List<string>();

            foreach (var audio in audiosList)
            {
                string[] words = audio.similarText.Split(','); 
                foreach (var word in words)
                {
                    if (!String.IsNullOrEmpty(word)) {
                        list.Add(word.TrimStart().TrimEnd()); // it cancels trim
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

        public static double matchStrings(string firstString, string secondString)
        {
            List<string> firstStringCouple = wordCharacterCouple(firstString.ToUpper());
            List<string> secondStringCouple = wordCharacterCouple(secondString.ToUpper());

            int intersectionCount = 0;
            int unionCount = firstStringCouple.Count + secondStringCouple.Count;

            for (int i = 0; i < firstStringCouple.Count; i++)
            {
                for (int j = 0; j < secondStringCouple.Count; j++)
                {
                    if (firstStringCouple[i] == secondStringCouple[j])
                    {
                        intersectionCount++;
                        secondStringCouple.RemoveAt(j);
                        break;
                    }
                }
            }

            return (2.0 * intersectionCount * 100) / unionCount;
        }

        private static List<string> wordCharacterCouple(string inputString)
        {
            List<string> allCharacterCouple = new List<string>();

            string[] words = Regex.Split(inputString, @"\s");

            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    String[] pairsInWord = characterCouple(words[i]);

                    for (int j = 0; j < pairsInWord.Length; j++)
                    {
                        allCharacterCouple.Add(pairsInWord[j]);
                    }
                }
            }
            return allCharacterCouple;
        }

        private static string[] characterCouple(string word)
        {
            int numberOfCouple = word.Length - 1;
            string[] pairs = new string[numberOfCouple];

            for (int i = 0; i < numberOfCouple; i++)
            {
                pairs[i] = word.Substring(i, 2);
            }
            return pairs;
        }
    }
}
