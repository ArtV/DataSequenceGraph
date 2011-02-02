using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using DataSequenceGraph.DataChunk;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void threeThreeRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary)
        {
            List<string> srcDataList = new List<string>() { "A", "B", "C" };
            StringDataChunk srcData = new StringDataChunk(srcDataList);
            List<string> srcData2List = new List<string>() { "A", "A", "D" };
            StringDataChunk srcData2 = new StringDataChunk(srcData2List);
            List<string> srcData3List = new List<string>() { "A", "A", "E" };
            StringDataChunk srcData3 = new StringDataChunk(srcData3List);

            DataChunkRouteBlazer<string> chunkRoute = new DataChunkRouteBlazer<string>(srcData, masterNodeList, routePrefixDictionary);
            DataChunkRouteBlazer<string> chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2, masterNodeList, routePrefixDictionary);
            DataChunkRouteBlazer<string> chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3, masterNodeList, routePrefixDictionary);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        static void threeSixRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary)
        {
            List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
            StringDataChunk srcData4 = new StringDataChunk(srcData4List);
            DataChunkRouteBlazer<string> chunkRoute4 =  new DataChunkRouteBlazer<string>(srcData4, masterNodeList, routePrefixDictionary);
            chunkRoute4.computeFullRoute();

            List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
            StringDataChunk srcData5 = new StringDataChunk(srcData5List);
            DataChunkRouteBlazer<string> chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5, masterNodeList, routePrefixDictionary);
            chunkRoute5.computeFullRoute();

            List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
            StringDataChunk srcData6 = new StringDataChunk(srcData6List);
            DataChunkRouteBlazer<string> chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6, masterNodeList, routePrefixDictionary);
            chunkRoute6.computeFullRoute();
        }

        static void Main(string[] args)
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node<string>,List<Route<string>>> routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();

//            threeThreeRoutes(masterNodeList, routePrefixDictionary);

            threeSixRoutes(masterNodeList, routePrefixDictionary);

            foreach (var node in masterNodeList.AllNodes)
            {
                string outStr = node.SequenceNumber + " " + node.GetType() + " ";
                if (node is ValueNode<string>)
                {
                    outStr += (node as ValueNode<string>).Value;
                }
                Console.Out.WriteLine(outStr);
                foreach(Route<string> route in node.OutgoingRoutes)
                {
                    outStr = route.connectedNodes.ElementAt(0).SequenceNumber + "," +
                        route.connectedNodes.ElementAt(1).SequenceNumber + 
                        " if already " + route.requisiteLinks.ElementAt(0).from.SequenceNumber + "," +
                        route.requisiteLinks.ElementAt(0).to.SequenceNumber;
                    Console.Out.WriteLine(outStr);
                }
            }
        }
    }
}
