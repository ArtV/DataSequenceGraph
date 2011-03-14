using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataSequenceGraph.Communication
{
    public class DeltaListFile
    {
        private List<string> deltaList;
        private string directoryPath;

        public DeltaListFile(IEnumerable<string> deltaList, string directoryPath)
        {
            this.deltaList = deltaList.ToList();
            this.directoryPath = directoryPath;
        }

        public string writeFile()
        {
            deltaList.Sort();
            deltaList.Reverse();
            if (deltaList.Count > 0)
            {                
                string topDelt = deltaList[0]; 
                string newFile = directoryPath + @"\" + topDelt + ".base";
                using (TextWriter textWriter = new StreamWriter(new FileStream(newFile, FileMode.Create)))
                {
                    textWriter.Write(topDelt);
                    foreach (string delta in deltaList.Skip(1))
                    {
                        textWriter.Write("|" + delta);
                    }
                }
                return newFile;
            }
            return null;
        }
    }
}
