using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DataSequenceGraph.Format;

namespace DataSequenceGraph.Communication
{
    public class DeltaDirectory
    {
        public string DirectoryPath { get; private set; }
        public List<string> allDeltaFilenames { get; private set; }
        public List<string> allFullGraphs { get; private set; }

        public string CurrentBase
        {
            get
            {
                return allDeltaFilenames[allDeltaFilenames.Count - 1];
            }
        }        

        public DeltaDirectory(string directoryPath)
        {
            this.DirectoryPath = directoryPath;

            reloadDirectory();
        }

        public void reloadDirectory()
        {
            string[] deltaFilenames = Directory.GetFiles(this.DirectoryPath, "*.dat");
            Array.Sort(deltaFilenames);
            this.allDeltaFilenames = new List<string>();
            this.allFullGraphs = new List<string>();
            string baseName;
            for (int i = 0; i <= deltaFilenames.Length - 1; i++)
            {
                // must check for .dat because of GetFiles 3-letter extension behavior
                if (!Path.GetExtension(deltaFilenames[i]).Equals(".dat"))
                {
                    continue;
                }
                baseName = Path.GetFileNameWithoutExtension(deltaFilenames[i]);
                if (baseName.IndexOf("delta") >= 0)
                {
                    this.allDeltaFilenames.Add(baseName);
                }
                else
                {
                    this.allFullGraphs.Add(baseName);
                }
            }            
        }

        public void initDirectory<NodeValType>(MasterNodeList<NodeValType> nodeList,
            NodeValueExporter<NodeValType> nodeValueExporter)
        {
            BinaryAndTXTFormat<NodeValType> fmt = setupFormat(
                nodeList.DataChunkCount.ToString("0000"),null, nodeValueExporter);
            fmt.ToBinaryAndTXTFiles(nodeList);

            fmt = setupFormat(DeltaList.generateNewDeltaFilename(0), null, nodeValueExporter);
            fmt.ToBinaryAndTXTFiles(nodeList);
        }

        public IEnumerable<string> getDeltasBeforeOrEqual(string otherDelta,int maxCount)
        {
            int returnCount = 0;
            for (int i = allDeltaFilenames.Count - 1; i >= 0 && returnCount < maxCount; i--)
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

        public IEnumerable<string> getDeltasAfterButUpTo(string exclusiveBase, string inclusiveLast)
        {
            return allDeltaFilenames.SkipWhile(fname => fname.CompareTo(exclusiveBase) <= 0).
                TakeWhile(fname => fname.CompareTo(inclusiveLast) <= 0);
        }

        public Tuple<int, string, int> findCommonBase(string[] otherDeltas)
        {
            string earliestCandidateBase = otherDeltas[0];
            int foundIndex = -1;
            int i = -1;
            for (i = 0; i <= otherDeltas.Length - 1; i++)
            {
                earliestCandidateBase = otherDeltas[i];
                foundIndex = allDeltaFilenames.FindIndex(fname => fname.Equals(earliestCandidateBase));
                if (foundIndex >= 0)
                {
                    break;
                }
            }
            return Tuple.Create(foundIndex,earliestCandidateBase,i);
        }

        public MasterNodeList<NodeValType> getLastFullGraph<NodeValType>(
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            return reconstructGraph(CurrentBase, nodeValueParser, nodeValueExporter);
        }

        public MasterNodeList<NodeValType> reconstructGraph<NodeValType>(string lastDeltaApplied,
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            var fullGraphInfo = getFullGraphBefore(lastDeltaApplied, nodeValueParser);
            string graphStemName;
            MasterNodeList<NodeValType> startingGraph;
            if (fullGraphInfo == null)
            {
                graphStemName = " ";
                startingGraph = new MasterNodeList<NodeValType>();
            }
            else
            {
                graphStemName = fullGraphInfo.Item1;
                startingGraph = fullGraphInfo.Item2;
            }
            applyDeltaSeriesToNodeList(getDeltasAfterButUpTo(graphStemName, lastDeltaApplied),
                startingGraph, nodeValueParser, nodeValueExporter);
            return startingGraph;
        }

        public Tuple<string,MasterNodeList<NodeValType>> getFullGraphBefore<NodeValType>(string delta,
              NodeValueParser<NodeValType> nodeValueParser)
        {
            string foundGraph = null;
            for (int i = allFullGraphs.Count - 1; i > 0; i--)
            {
                if (allFullGraphs.ElementAt(i).CompareTo(delta) < 0)
                {
                    foundGraph = allFullGraphs.ElementAt(i);
                    break;
                }
            }
            if (foundGraph == null)
            {
                return null;
            }
            BinaryAndTXTFormat<NodeValType> fmt = setupFormat(foundGraph, nodeValueParser, null);
            return Tuple.Create(foundGraph,fmt.ToNodeListFromFiles());
        }

        public void applyDeltaSeriesToNodeList<NodeValType>(IEnumerable<string> deltaSeries,
            MasterNodeList<NodeValType> nodeList, NodeValueParser<NodeValType> nodeValueParser,
            NodeValueExporter<NodeValType> nodeValueExporter)
        {
            foreach (string delt in deltaSeries)
            {
                applyDeltaToNodeList(delt, nodeList, nodeValueParser, nodeValueExporter);
            }
        }

        public void applyDeltaToNodeList<NodeValType>(string whichDelta, MasterNodeList<NodeValType> nodeList, 
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            BinaryAndTXTFormat<NodeValType> fmt = setupFormat(whichDelta, nodeValueParser, nodeValueExporter);
            fmt.ToNodeListFromFiles(nodeList);

            BinaryAndTXTFormat<NodeValType> newBaseFmt = setupFormat(
                nodeList.DataChunkCount.ToString("0000"), nodeValueParser, nodeValueExporter);
            newBaseFmt.ToBinaryAndTXTFiles(nodeList);
        }

        public void dumpAsFullGraph<NodeValType>(MasterNodeList<NodeValType> nodeList,
            NodeValueExporter<NodeValType> nodeValueExporter)
        {
            BinaryAndTXTFormat<NodeValType> newBaseFmt = setupFormat(
                nodeList.DataChunkCount.ToString("0000"), null, nodeValueExporter);
            newBaseFmt.ToBinaryAndTXTFiles(nodeList);
        }

        private BinaryAndTXTFormat<NodeValType> setupFormat<NodeValType>(string whichStemName,
            NodeValueParser<NodeValType> nodeValueParser, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            BinaryAndTXTFormat<NodeValType> fmt = new BinaryAndTXTFormat<NodeValType>(
                DirectoryPath + @"\" + whichStemName + ".dat",
                DirectoryPath + @"\" + whichStemName + ".txt",
                nodeValueParser);
            fmt.nodeValueExporter = nodeValueExporter;
            return fmt;
        }

        public void junkFilesAfter(string exclusiveStart)
        {
            string[] allFilenames = Directory.GetFiles(this.DirectoryPath);
            Dictionary<string, string> oldToNewExtension = new Dictionary<string, string>()
            {
                { ".dat", ".datx" },
                { ".txt", ".txtx" }
            };
            foreach (string fname in allFilenames)
            {
                if (Path.GetExtension(fname).Equals(".dat") || 
                    Path.GetExtension(fname).Equals(".txt"))
                {
                    if (Path.GetFileNameWithoutExtension(fname).CompareTo(exclusiveStart) > 0)
                    {
                        File.Move(fname, fname.Substring(0,fname.Length - 4) + 
                            oldToNewExtension[Path.GetExtension(fname)]);
                    }
                }
            }
            reloadDirectory();
        }
    }
}
