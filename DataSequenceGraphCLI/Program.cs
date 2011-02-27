using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using DataSequenceGraph.Format;
using System.Xml;
using System.IO;
using DataSequenceGraph.Chunk;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void threeThreeRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcDataList = new List<string>() { "A", "B", "C" };
            List<string> srcData2List = new List<string>() { "A", "A", "D" };
            List<string> srcData3List = new List<string>() { "A", "A", "E" };

            DataChunkRouteBlazer<string> chunkRoute = new DataChunkRouteBlazer<string>(srcDataList, masterNodeList);
            DataChunkRouteBlazer<string> chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2List, masterNodeList);
            DataChunkRouteBlazer<string> chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3List, masterNodeList);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        static void threeSixRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
            DataChunkRouteBlazer<string> chunkRoute4 = new DataChunkRouteBlazer<string>(srcData4List, masterNodeList);
            chunkRoute4.computeFullRoute();

            List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
            DataChunkRouteBlazer<string> chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5List, masterNodeList);
            chunkRoute5.computeFullRoute();

            List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
            DataChunkRouteBlazer<string> chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6List, masterNodeList);
            chunkRoute6.computeFullRoute();
        }

        static void Main(string[] args)
        {
            MasterNodeList<string> masterNodeList = null;
            if (args.Length >= 1 && args[0].ToUpper() == "L")
            {
                masterNodeList = loadFile(args[1]);

            using (TextReader reader = new StreamReader(new FileStream("common_follow2.txt", FileMode.Open, FileAccess.Read)))
            {
                var sentences = SentenceChunkLoader.ToSentenceChunks(reader);
                foreach (string sentence in sentences)
                {
                    var words = SentenceChunkLoader.ToWordValues(sentence);
                    var blazer = new DataChunkRouteBlazer<string>(words, masterNodeList);
                    blazer.computeFullRoute();
                }
            }

                MasterNodeList<string> masterNodeList2 = loadFile(args[1]);
                DataChunkRoute<string> followRoute = masterNodeList.enumerateDataChunkRoutes().Last();

                BinaryAndTXTFormat<string> format = new BinaryAndTXTFormat<string>("nodesEdges2.dat", "values2.txt");
//                XMLGraphFormat<string> format = new XMLGraphFormat<string>("follow.xml");
                format.nodeValueParser = new StringNodeValueParser();
//                var missing = followRoute.specsForMissingComponents(masterNodeList2);
//                format.ToXMLFile(masterNodeList2, missing.Item1, missing.Item2);
//                format.ToBinaryAndTXTFiles(masterNodeList2,missing.Item1,missing.Item2);
                var missing = followRoute.comboSpecsForMissingComponents(masterNodeList2);
                nodeAndReqOutput(missing);
                format.ToBinaryAndTXTFiles(masterNodeList2, missing);

                
                
                if (args.Length >= 3 && args[2].ToUpper() == "F")
                {
                    binaryTEXTFiles(masterNodeList);
                }
                else
                {
                    defaultTestOutput(masterNodeList);
                    printEnumeratedChunks(masterNodeList);
                } 
            }
            else if (args.Length >= 2 && args[1] == "3")
            {
                masterNodeList = setupNodeList33();
            }
            else if (args.Length >= 2 && args[1].ToUpper() == "R")
            {
                BinaryAndTXTFormat<string> format = new BinaryAndTXTFormat<string>("nodesEdges.dat", "values.txt");
                format.nodeValueParser = new StringNodeValueParser();
                masterNodeList = format.ToNodeListFromFiles();
            }
            else
            {
                masterNodeList = setupNodeList();
            }
            if (args.Length == 0 || (args.Length >= 1 && args[0].ToUpper() == "D"))
            {
                defaultTestOutput(masterNodeList);
                printEnumeratedChunks(masterNodeList);
            }
            else if (args[0].ToUpper() == "X")
            {
                XMLOut(masterNodeList);
            }
        }

        static void binaryTEXTFiles(MasterNodeList<string> nodeList)
        {
            BinaryAndTXTFormat<string> format = new BinaryAndTXTFormat<string>("nodesEdges.dat", "values.txt");
            format.nodeValueParser = new StringNodeValueParser();
            format.ToBinaryAndTXTFiles(nodeList);
        }        

        static MasterNodeList<string> loadFile(string filename)
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> routePrefixDictionary = new Dictionary<Node, List<Route>>();
            List<string> sentences;
            List<string> words;
            DataChunkRouteBlazer<string> blazer;
            using (TextReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                sentences = SentenceChunkLoader.ToSentenceChunks(reader);
            }
            foreach (string sentence in sentences)
            {
                words = SentenceChunkLoader.ToWordValues(sentence);
                blazer = new DataChunkRouteBlazer<string>(words, masterNodeList);
                blazer.computeFullRoute();
            }
            return masterNodeList;
        }

        static void nodeAndReqOutput(IEnumerable<NodeAndReqSpec> specs)
        {
            foreach(NodeAndReqSpec spec in specs)
            {
                Console.Out.WriteLine(spec.fromNode.SequenceNumber + "," + spec.insertFrom + 
                    "," + spec.ReqFromSequenceNumber + "," + spec.ReqToSequenceNumber);
            }
        }

        static void defaultTestOutput(MasterNodeList<string> masterNodeList)
        {
            foreach (var node in masterNodeList.AllNodes)
            {
                string outStr = node.SequenceNumber + " " + node.GetType() + " ";
                if (node.kind == NodeKind.ValueNode)
                {
                    outStr += (node as ValueNode<string>).Value;
                }
                Console.Out.WriteLine(outStr);
                foreach (EdgeRoute route in node.OutgoingEdges)
                {
                    outStr = route.connectedNodes.ElementAt(0).SequenceNumber + "," +
                        route.connectedNodes.ElementAt(1).SequenceNumber +
                        " if already " + route.edge.requisiteLink.from.SequenceNumber + "," +
                        route.edge.requisiteLink.to.SequenceNumber;
                    Console.Out.WriteLine(outStr);
                }
            }

        }

        static void printEnumeratedChunks(MasterNodeList<string> masterNodeList)
        {            
            foreach (DataChunkRoute<string> route in masterNodeList.enumerateDataChunkRoutes())
            {
                ValueNode<string> lastNode = null;
                foreach (EdgeRoute edge in route.componentEdges)
                {
                    Console.Out.Write(edge.edge.link.from.SequenceNumber);
                    if (edge.edge.link.from is ValueNode<string>)
                    {
                        Console.Out.Write("," + ((ValueNode<string>)edge.edge.link.from).Value);
                    }
                    Console.Out.Write("  ");
                    lastNode = edge.edge.link.to as ValueNode<string>;
                }
                Console.Out.Write(lastNode.SequenceNumber + "," + lastNode.Value);
                Console.Out.WriteLine();
                Console.Out.WriteLine();
            }
        }

        static MasterNodeList<string> setupNodeList()
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> routePrefixDictionary = new Dictionary<Node, List<Route>>();

            threeSixRoutes(masterNodeList, routePrefixDictionary);
            return masterNodeList;
        }

        static MasterNodeList<string> setupNodeList33()
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> routePrefixDictionary = new Dictionary<Node, List<Route>>();

            threeThreeRoutes(masterNodeList, routePrefixDictionary);
            return masterNodeList;
        }

        static void XMLOut(MasterNodeList<string> masterNodeList)
        {
            XMLGraphFormat<string> formatter = new XMLGraphFormat<string>();
            formatter.nodeValueParser = new StringNodeValueParser();

            XmlDocument doc = formatter.ToXMLDocument(masterNodeList);

            doc.WriteContentTo(new XmlTextWriter(Console.Out));

            masterNodeList = formatter.ToNodeList(new XmlTextReader(new StringReader(doc.OuterXml)));

            defaultTestOutput(masterNodeList);
            printEnumeratedChunks(masterNodeList);
        }      
    }
}
