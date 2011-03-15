using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph.Format;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace DataSequenceGraph.Communication
{
    public enum DeltaResponseResultKind { UpdateRequest,Dismissal,Acceptance }

    public class DeltaResponseHandler
    {
        public static DeltaResponseResultKind handleDeltaBaseResponse(DeltaDirectory deltaDirectory,
            TextReader deltaBaseResponse,TextWriter updateRequestWriter)
        {
            DeltaResponseResultKind result = DeltaResponseResultKind.Dismissal;

            string[] allDeltas = DeltaList.readList(deltaBaseResponse);
            string actualRequestBase = allDeltas[0];
            string currentBase = deltaDirectory.CurrentBase;

            int comp = actualRequestBase.CompareTo(currentBase);
            if (comp != 0)
            {
                result = DeltaResponseResultKind.UpdateRequest;
                var foundBase = deltaDirectory.findCommonBase(allDeltas);               
                new DeltaRequest(deltaDirectory).writeRequestUsingDelta(
                    foundBase.Item2,updateRequestWriter);
            }
            return result;
        }

        // NOTE: caller must execute one of the add*Chunks methods, and keep around a copy
        //  of the old base to reapply local chunks after the base is updated here
        public static DeltaResponseResultKind handleDeltaArchiveResponse<NodeValType>(DeltaDirectory deltaDirectory,
            MasterNodeList<NodeValType> baseNodeList,NodeValueParser<NodeValType> nodeValueParser,
            NodeValueExporter<NodeValType> nodeValueExporter, Stream deltaArchiveResponse)
        {
            DeltaResponseResultKind result = DeltaResponseResultKind.Dismissal;

            string tempDir = extractDeltaArchive(deltaArchiveResponse);

            string[] baseFiles = Directory.GetFiles(tempDir, "*.base");
            string[] allDeltas = null;
            using (TextReader textReader = new StreamReader(new FileStream(baseFiles[0], FileMode.Open, FileAccess.Read)))
            {
                allDeltas = DeltaList.readList(textReader);
            }
            string actualRequestBase = allDeltas[0];
            string currentBase = deltaDirectory.CurrentBase;

            var foundBase = deltaDirectory.findCommonBase(allDeltas);
            if (foundBase.Item1 >= 0)
            {
                int foundIndex = foundBase.Item1;
                string commonBase = foundBase.Item2;
                if (commonBase.Equals(currentBase))
                {
                    result = DeltaResponseResultKind.Acceptance;
                    copyAndApplyDeltas(tempDir,deltaDirectory,baseNodeList,nodeValueParser,nodeValueExporter);
                }
                else
                {
                }
            }

            return result;
        }

        private static string extractDeltaArchive(Stream deltaArchive)
        {
            TarArchive arch = TarArchive.CreateInputTarArchive(new GZipInputStream(deltaArchive));
            string myTempPath = Path.GetTempPath() + @"\dsg" + DateTime.Now.Ticks;
            Directory.CreateDirectory(myTempPath);
            arch.ExtractContents(myTempPath);
            arch.Close();
            return myTempPath;
        }

        private static void copyAndApplyDeltas<NodeValType>(string srcPath, DeltaDirectory deltaDir, MasterNodeList<NodeValType> baseNodeList,
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            string[] datFiles = Directory.GetFiles(srcPath, "*.dat");
            Array.Sort(datFiles);
            int baseIndexBefore = baseNodeList.DataChunkCount;                
            foreach (string datFileName in datFiles)
            {
                string txtFileName = datFileName.Substring(0,datFileName.Length - 3) + "txt";
                BinaryAndTXTFormat<NodeValType> fmt = new BinaryAndTXTFormat<NodeValType>(datFileName, txtFileName);
                fmt.nodeValueParser = nodeValueParser;
                fmt.ToNodeListFromFiles(baseNodeList);

                File.Move(datFileName, deltaDir.DirectoryPath + @"\" + Path.GetFileName(datFileName));
                File.Move(txtFileName, deltaDir.DirectoryPath + @"\" + Path.GetFileName(txtFileName));
            }
            string newBaseStemFileName = baseNodeList.DataChunkCount.ToString("0000");
            BinaryAndTXTFormat<NodeValType> newBaseFmt = new BinaryAndTXTFormat<NodeValType>(
                deltaDir.DirectoryPath + @"\" + newBaseStemFileName + ".dat",
                deltaDir.DirectoryPath + @"\" + newBaseStemFileName + ".txt",
                nodeValueExporter);
            newBaseFmt.ToBinaryAndTXTFiles(baseNodeList);
        }
    }
}
