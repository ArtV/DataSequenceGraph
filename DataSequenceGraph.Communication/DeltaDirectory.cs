using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataSequenceGraph.Communication
{
    public class DeltaDirectory
    {
        public string DirectoryPath { get; private set; }
        private string[] allDeltaFilenames;

        public string CurrentBase
        {
            get
            {
                return allDeltaFilenames[allDeltaFilenames.Length - 1];
            }
        }

        public DeltaDirectory(string directoryPath)
        {
            this.DirectoryPath = directoryPath;

            string[] deltaFilenames = Directory.GetFiles(directoryPath, "*.dat");
            Array.Sort(deltaFilenames);
            this.allDeltaFilenames = new string[deltaFilenames.Length];
            for (int i = 0; i <= deltaFilenames.Length - 1; i++)
            {
                this.allDeltaFilenames[i] = Path.GetFileNameWithoutExtension(deltaFilenames[i]);
            }            
        }

        public IEnumerable<string> getDeltasBefore(string otherDelta)
        {
            return allDeltaFilenames.TakeWhile(fname => fname.CompareTo(otherDelta) <= 0);
        }

        public IEnumerable<string> getDeltasAfter(string otherDelta)
        {
            return allDeltaFilenames.SkipWhile(fname => fname.CompareTo(otherDelta) < 0);
        }
    }
}
