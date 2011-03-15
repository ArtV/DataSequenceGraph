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
        public string[] allDeltaFilenames { get; private set; }

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

            reloadDirectory();
        }

        public void reloadDirectory()
        {
            string[] deltaFilenames = Directory.GetFiles(this.DirectoryPath, "*delta*.dat");
            Array.Sort(deltaFilenames);
            this.allDeltaFilenames = new string[deltaFilenames.Length];
            for (int i = 0; i <= deltaFilenames.Length - 1; i++)
            {
                this.allDeltaFilenames[i] = Path.GetFileNameWithoutExtension(deltaFilenames[i]);
            }            
        }

        public IEnumerable<string> getDeltasBeforeOrEqual(string otherDelta,int maxCount)
        {
            int returnCount = 0;
            for (int i = allDeltaFilenames.Length - 1; i >= 0 && returnCount < maxCount; i--)
            {
                if (allDeltaFilenames[i].CompareTo(otherDelta) <= 0)
                {
                    yield return allDeltaFilenames[i];
                    returnCount++;
                }
            }
        }

        public IEnumerable<string> getDeltasAfter(string otherDelta)
        {
            return allDeltaFilenames.SkipWhile(fname => fname.CompareTo(otherDelta) <= 0);
        }

        public Tuple<int, string> findCommonBase(string[] otherDeltas)
        {
            string earliestCandidateBase = otherDeltas[0];
            int foundIndex = -1;
            for (int i = 0; i <= otherDeltas.Length - 1; i++)
            {
                earliestCandidateBase = otherDeltas[i];
                foundIndex = Array.BinarySearch(allDeltaFilenames, earliestCandidateBase);
                if (foundIndex >= 0)
                {
                    break;
                }
            }
            return Tuple.Create(foundIndex,earliestCandidateBase);
        }
    }
}
