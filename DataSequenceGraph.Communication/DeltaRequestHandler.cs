using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace DataSequenceGraph.Communication
{
    public enum ResultKind { Empty, Mismatch, Deltas }

    public class DeltaRequestHandler
    {
        public static ResultKind handleDeltaRequest(DeltaDirectory deltaDirectory,TextReader textReader,Stream outS)
        {
            ResultKind returnResult = ResultKind.Empty;

            string[] allDeltas = DeltaList.readList(textReader);
            string actualRequestBase = allDeltas[0];
            string currentBase = deltaDirectory.CurrentBase;

            int comp = actualRequestBase.CompareTo(currentBase);
            if (comp > 0)
            {
                returnResult = ResultKind.Empty;
                DeltaRequest.createAndWrite(deltaDirectory, new StreamWriter(outS));
            }
            else if (comp < 0)
            {
                string earliestCandidateBase = actualRequestBase;
                bool foundCommonBase = false;
                int foundIndex;
                for(int i = 0; i <= allDeltas.Length - 1; i++)
                {
                    earliestCandidateBase = allDeltas[i];
                    foundIndex = Array.BinarySearch(deltaDirectory.allDeltaFilenames, earliestCandidateBase);
                    if (foundIndex >= 0)
                    {
                        foundCommonBase = true;
                        break;
                    }
                }
                if (foundCommonBase) 
                {
                    returnResult = ResultKind.Deltas;
                    writeDeltaArchive(deltaDirectory, outS, earliestCandidateBase);
                }
                else
                {
                    returnResult = ResultKind.Mismatch;
                    DeltaList.writeList(deltaDirectory.getDeltasBeforeOrEqual(earliestCandidateBase, 5),
                        new StreamWriter(outS));
                }
            }
            else // TODO compute and send brand-new delta 
            { /*  
               * 
               *  create new delta file then call writeDeltaArchive with currentBase?????
               *  careful, creating new delta file changes currentBase...
                result.kind = ResultKind.Empty;
                DeltaRequest.createAndWrite(deltaDirectory, new StreamWriter(result.outS));
               */
            }

            return returnResult;
        }

        public static void writeDeltaArchive(DeltaDirectory deltaDirectory, Stream outStream,
            string requestedBase)
        {
            IEnumerable<string> sentDeltas = deltaDirectory.getDeltasAfter(requestedBase);

            TarArchive requestTar = TarArchive.CreateOutputTarArchive(new GZipOutputStream(outStream));

            string baseFile = deltaDirectory.DirectoryPath + @"\" + requestedBase + ".base";
            using (TextWriter textWriter = new StreamWriter(new FileStream(baseFile, FileMode.Create)))
            {
                DeltaList.writeList(sentDeltas, textWriter);
            }            
            requestTar.WriteEntry(TarEntry.CreateEntryFromFile(baseFile), false);

            foreach (string delta in sentDeltas)
            {
                string deltaDat = deltaDirectory.DirectoryPath + @"\" + delta + ".dat";
                requestTar.WriteEntry(TarEntry.CreateEntryFromFile(deltaDat), false);
                string deltaTxt = deltaDirectory.DirectoryPath + @"\" + delta + ".txt";
                requestTar.WriteEntry(TarEntry.CreateEntryFromFile(deltaTxt), false);
            }

            requestTar.Close();
            File.Delete(baseFile);
        }
    }
}
