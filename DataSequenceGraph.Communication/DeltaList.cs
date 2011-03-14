using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataSequenceGraph.Communication
{
    public class DeltaList
    {
        public static void writeList(IEnumerable<string> deltaIEnum, TextWriter outWriter)
        {
            List<string> deltaList = deltaIEnum.ToList();
            deltaList.Sort();
            deltaList.Reverse();
            if (deltaList.Count > 0)
            {                
                string topDelt = deltaList[0];
                outWriter.Write(topDelt);
                foreach (string delta in deltaList.Skip(1))
                {
                    outWriter.Write("|" + delta);
                }                
            }
            outWriter.Flush();
        }

        public static string[] readList(TextReader inReader)
        {
            string entireText = inReader.ReadToEnd();
            return entireText.Split(new char[] { '|' });
        }
    }
}
