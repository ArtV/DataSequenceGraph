using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using DataSequenceGraph;
using DataSequenceGraph.Format;

namespace DataSequenceGraph.Communication
{
    public enum ResultKind { Empty, Mismatch, Deltas }

    public class DeltaRequestHandler
    {
        public static ResultKind handleDeltaRequest<NodeValType>(DeltaDirectory deltaDirectory,MasterNodeList<NodeValType> baseNodeList,
            MasterNodeList<NodeValType> localNodeList,NodeValueExporter<NodeValType> nodeValueExporter,
            TextReader textReader,Stream outS)
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
            else
            {
                returnResult = ResultKind.Deltas;
                IList<NodeAndReqSpec> nodeReqSpecs = localNodeList.getSpecsAbsentIn(baseNodeList);
                string stemDeltaFilename = baseNodeList.DataChunkCount.ToString("0000") +
                    "-" + DateTime.Now.ToString("yyyy-MM-dd'T'HH-mm-ss'Z'");
                string stemNameWithPath = deltaDirectory.DirectoryPath + @"\" + stemDeltaFilename;
                BinaryAndTXTFormat<NodeValType> fmt =
                    new BinaryAndTXTFormat<NodeValType>(stemNameWithPath + ".dat", stemNameWithPath + ".txt",
                        nodeValueExporter);
                fmt.ToBinaryAndTXTFiles(localNodeList, baseNodeList, nodeReqSpecs);
                writeDeltaArchive(new List<string> { stemDeltaFilename }, deltaDirectory.DirectoryPath, 
                    outS, actualRequestBase);
                // NOTE caller should overwrite base node list with local node list!!!
            }

            return returnResult;
        }       

        public static void writeDeltaArchive(DeltaDirectory deltaDirectory, Stream outStream,
            string requestedBase)
        {
            IEnumerable<string> sentDeltas = deltaDirectory.getDeltasAfter(requestedBase);

            writeDeltaArchive(sentDeltas, deltaDirectory.DirectoryPath, outStream, requestedBase);
        }

        public static void writeDeltaArchive(IEnumerable<string> sentDeltas, string deltaPath,
            Stream outStream, string requestedBase)
        {
            TarArchive requestTar = TarArchive.CreateOutputTarArchive(new GZipOutputStream(outStream));

            string baseFile = deltaPath + @"\" + requestedBase + ".base";
            using (TextWriter textWriter = new StreamWriter(new FileStream(baseFile, FileMode.Create)))
            {
                DeltaList.writeList(new List<string> { requestedBase }, textWriter);
            }
            requestTar.WriteEntry(TarEntry.CreateEntryFromFile(baseFile), false);

            foreach (string delta in sentDeltas)
            {
                string deltaDat = deltaPath + @"\" + delta + ".dat";
                requestTar.WriteEntry(TarEntry.CreateEntryFromFile(deltaDat), false);
                string deltaTxt = deltaPath + @"\" + delta + ".txt";
                requestTar.WriteEntry(TarEntry.CreateEntryFromFile(deltaTxt), false);
            }

            requestTar.Close();
            File.Delete(baseFile);
        }
    }
}
