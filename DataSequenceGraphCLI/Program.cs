using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using DataSequenceGraph.Format;
using System.Xml;
using System.IO;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void threeThreeRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcDataList = new List<string>() { "A", "B", "C" };
            List<string> srcData2List = new List<string>() { "A", "A", "D" };
            List<string> srcData3List = new List<string>() { "A", "A", "E" };

            DataChunkRouteBlazer<string> chunkRoute = new DataChunkRouteBlazer<string>(srcDataList, masterNodeList, routePrefixDictionary);
            DataChunkRouteBlazer<string> chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2List, masterNodeList, routePrefixDictionary);
            DataChunkRouteBlazer<string> chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3List, masterNodeList, routePrefixDictionary);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        static void threeSixRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
            DataChunkRouteBlazer<string> chunkRoute4 =  new DataChunkRouteBlazer<string>(srcData4List, masterNodeList, routePrefixDictionary);
            chunkRoute4.computeFullRoute();

            List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
            DataChunkRouteBlazer<string> chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5List, masterNodeList, routePrefixDictionary);
            chunkRoute5.computeFullRoute();

            List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
            DataChunkRouteBlazer<string> chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6List, masterNodeList, routePrefixDictionary);
            chunkRoute6.computeFullRoute();
        }

        static void Main(string[] args)
        {
            MasterNodeList<string> masterNodeList;
            if (args.Length >= 2 && args[1] == "3")
            {
                masterNodeList = setupNodeList33();
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
                foreach (Route route in node.OutgoingRoutes)
                {
                    outStr = route.connectedNodes.ElementAt(0).SequenceNumber + "," +
                        route.connectedNodes.ElementAt(1).SequenceNumber +
                        " if already " + route.requisiteLinks.ElementAt(0).from.SequenceNumber + "," +
                        route.requisiteLinks.ElementAt(0).to.SequenceNumber;
                    Console.Out.WriteLine(outStr);
                }
            }

        }

        static void printEnumeratedChunks(MasterNodeList<string> masterNodeList)
        {
            IEnumerable<IEnumerable<string>> outChunks = masterNodeList.enumerateDataChunks();
            foreach (IEnumerable<string> chunk in outChunks)
            {
                foreach (string val in chunk)
                {
                    Console.Out.Write(val);
                }
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

            XmlDocument doc = formatter.ToXML(masterNodeList);

            doc.WriteContentTo(new XmlTextWriter(Console.Out));

            masterNodeList = formatter.ToNodeList(new XmlTextReader(new StringReader(doc.OuterXml)));

            defaultTestOutput(masterNodeList);
            printEnumeratedChunks(masterNodeList);
        }      
    }
}
