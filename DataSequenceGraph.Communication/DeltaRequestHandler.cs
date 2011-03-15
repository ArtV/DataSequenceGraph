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
    public enum DeltaRequestResultKind { Empty, Mismatch, Deltas }

    public class DeltaRequestHandler
    {
        public static DeltaRequestResultKind handleDeltaRequest<NodeValType>(DeltaDirectory deltaDirectory,MasterNodeList<NodeValType> baseNodeList,
            MasterNodeList<NodeValType> localNodeList,NodeValueExporter<NodeValType> nodeValueExporter,
            TextReader requestReader,Stream outS)
        {
            DeltaRequestResultKind returnResult = DeltaRequestResultKind.Empty;

            string[] allDeltas = DeltaList.readList(requestReader);
            string actualRequestBase = allDeltas[0];
            string currentBase = deltaDirectory.CurrentBase;

            int comp = actualRequestBase.CompareTo(currentBase);
            if (comp > 0)
            {
                returnResult = DeltaRequestResultKind.Empty;
                new DeltaRequest(deltaDirectory).writeDefaultRequest(new StreamWriter(outS));
            }
            else if (comp < 0)
            { 
                Tuple<int, string> foundBase = deltaDirectory.findCommonBase(allDeltas);
                if (foundBase.Item1 >= 0)
                {
                    returnResult = DeltaRequestResultKind.Deltas;
                    writeDeltaArchive(deltaDirectory, outS, foundBase.Item2);
                }
                else
                {
                    returnResult = DeltaRequestResultKind.Mismatch;
                    DeltaList.writeList(deltaDirectory.getDeltasBeforeOrEqual(foundBase.Item2, 5),
                        new StreamWriter(outS));
                }
            }
            else
            {
                returnResult = DeltaRequestResultKind.Deltas;
                int beforeBaseNodeCount = baseNodeList.DataChunkCount;
                IList<NodeAndReqSpec> nodeReqSpecs = localNodeList.getSpecsAbsentIn(baseNodeList);
                string stemDeltaFilename = beforeBaseNodeCount.ToString("0000") +
                    "-delta-" + DateTime.Now.ToString("yyyy-MM-dd'T'HH-mm-ss'Z'");
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

        private static void writeDeltaArchive(DeltaDirectory deltaDirectory, Stream outStream,
            string requestedBase)
        {
            IEnumerable<string> sentDeltas = deltaDirectory.getDeltasAfter(requestedBase);

            writeDeltaArchive(sentDeltas, deltaDirectory.DirectoryPath, outStream, requestedBase);
        }

        private static void writeDeltaArchive(IEnumerable<string> sentDeltas, string deltaPath,
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
