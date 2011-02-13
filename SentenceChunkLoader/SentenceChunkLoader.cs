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
            foreach(Match match in Regex.Matches(originalString,sentencePattern))
            {
                sentences.Add(match.Groups[1].Value);
            }
            return sentences;
        }
    }
}
