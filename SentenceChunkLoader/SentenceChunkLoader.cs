using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DataSequenceGraph.Chunk
{
    public class SentenceChunkLoader
    {
        public static List<string> ToSentenceChunks(TextReader incomingText)
        {
            List<string> sentences = new List<string>();
            string originalString;
            using (incomingText)
            {
                originalString = incomingText.ReadToEnd();
            }            
            string sentencePattern = @"(\w[^.|?|!]+)(?:\.|\?|!)+";
            foreach(Match match in Regex.Matches(originalString,sentencePattern,RegexOptions.Compiled))
            {
                sentences.Add(match.Groups[1].Value);
            }
            return sentences;
        }

        public static List<string> ToWordValues(string sentence)
        {
            List<string> words = new List<string>();
            string wordsPattern = @"\b(\w+?)\b";
            foreach (Match match in Regex.Matches(sentence, wordsPattern, RegexOptions.Compiled))
            {
                words.Add(match.Groups[1].Value);
            }
            return words;
        }
    }
}
