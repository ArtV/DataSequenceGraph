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
    public enum DeltaResponseResultKind { UpdateRequest,Dismissal,Acceptance,Rewrite }

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

        public static DeltaResponseResultKind
            handleDeltaArchiveResponse<NodeValType>(MasterNodeList<NodeValType> originalLocalNodeList,
            DeltaDirectory deltaDirectory,
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter,
            Stream deltaArchiveResponse, out MasterNodeList<NodeValType> newLocalNodeList)
        {
            return handleDeltaArchiveResponse(originalLocalNodeList, deltaDirectory,
                deltaDirectory.getLastFullGraph(nodeValueParser,nodeValueExporter), nodeValueParser,
                nodeValueExporter, deltaArchiveResponse, out newLocalNodeList);
        }

        public static DeltaResponseResultKind 
            handleDeltaArchiveResponse<NodeValType>(MasterNodeList<NodeValType> originalLocalNodeList,
            DeltaDirectory deltaDirectory,MasterNodeList<NodeValType> baseNodeList,
            NodeValueParser<NodeValType> nodeValueParser,NodeValueExporter<NodeValType> nodeValueExporter, 
            Stream deltaArchiveResponse,out MasterNodeList<NodeValType> newLocalNodeList)
        {
            DeltaResponseResultKind result = DeltaResponseResultKind.Dismissal;
            newLocalNodeList = null;

            string tempDir = extractDeltaArchive(deltaArchiveResponse);
            string[] datFiles = Directory.GetFiles(tempDir, "*.dat");
            Array.Sort(datFiles);
            // must check for .dat because of GetFiles behavior for 3-letter extensions
            string theirCandidateDelta = Path.GetFileNameWithoutExtension(
                datFiles.First(fname => Path.GetExtension(fname).Equals(".dat")));
            string[] baseFiles = Directory.GetFiles(tempDir, "*.base");
            string actualRequestBase = null;
            using (TextReader textReader = new StreamReader(new FileStream(baseFiles[0], FileMode.Open, FileAccess.Read)))
            {
                actualRequestBase = DeltaList.readList(textReader)[0];
            }
            string currentBase = deltaDirectory.CurrentBase;

            var foundBase = deltaDirectory.findCommonBase(new string[] { actualRequestBase });
            if (foundBase.Item1 >= 0)
            {
                int foundIndex = foundBase.Item1;
                string commonBase = foundBase.Item2;
                if (commonBase.Equals(currentBase))
                {
                    result = DeltaResponseResultKind.Acceptance;
                    int baseIndexBefore = baseNodeList.DataChunkCount;                
                    copyAndApplyDeltas(tempDir,deltaDirectory,baseNodeList,nodeValueParser,nodeValueExporter);
                    originalLocalNodeList.addChunksStartingAtIndexToOtherList(baseIndexBefore, baseNodeList);
                    newLocalNodeList = baseNodeList;
                }
                else
                {
                    string myCandidateDelta = deltaDirectory.allDeltaFilenames[foundIndex + 1];
                    if (myCandidateDelta.CompareTo(theirCandidateDelta) < 0)
                    {
                        result = DeltaResponseResultKind.Dismissal;
                    }
                    else
                    {
                        result = DeltaResponseResultKind.Rewrite;
                        MasterNodeList<NodeValType> oldCommonBase = deltaDirectory.reconstructGraph(
                            commonBase, nodeValueParser, nodeValueExporter);
                        int baseIndexBefore = oldCommonBase.DataChunkCount;
                        deltaDirectory.junkFilesAfter(commonBase);
                        copyAndApplyDeltas(tempDir, deltaDirectory, oldCommonBase, nodeValueParser, nodeValueExporter);
                        originalLocalNodeList.addChunksStartingAtIndexToOtherList(baseIndexBefore, oldCommonBase);
                        newLocalNodeList = oldCommonBase;
                    }
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

            foreach (string datFileName in datFiles)
            {
                // must check for .dat because of GetFiles 3-letter extension behavior
                if (!Path.GetExtension(datFileName).Equals(".dat"))
                {
                    continue;
                }
                string stemName = Path.GetFileName(datFileName);
                stemName = stemName.Substring(0,stemName.Length - 4);
                string txtFileName = datFileName.Substring(0,datFileName.Length - 3) + "txt";

                File.Move(datFileName, deltaDir.DirectoryPath + @"\" + Path.GetFileName(datFileName));
                File.Move(txtFileName, deltaDir.DirectoryPath + @"\" + Path.GetFileName(txtFileName));
                deltaDir.applyDeltaToNodeList(stemName, baseNodeList,nodeValueParser, nodeValueExporter);
            }
        }
    }
}
