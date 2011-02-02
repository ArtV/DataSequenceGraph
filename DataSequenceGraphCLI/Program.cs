using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void threeThreeRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary)
        {
            List<string> srcData = new List<string>() { "A", "B", "C" };
            List<string> srcData2 = new List<string>() { "A", "A", "D" };
            List<string> srcData3 = new List<string>() { "A", "D", "E" };

            DataChunkRoute<string> chunkRoute = new DataChunkRoute<string>(srcData, masterNodeList, routePrefixDictionary);
            DataChunkRoute<string> chunkRoute2 = new DataChunkRoute<string>(srcData2, masterNodeList, routePrefixDictionary);
            DataChunkRoute<string> chunkRoute3 = new DataChunkRoute<string>(srcData3, masterNodeList, routePrefixDictionary);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        static void twoFiveRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary)
        {
            List<string> srcData4 = new List<string>() { "A", "B", "C", "D", "E" };
            DataChunkRoute<string> chunkRoute4 =  new DataChunkRoute<string>(srcData4, masterNodeList, routePrefixDictionary);
            chunkRoute4.computeFullRoute();

            List<string> srcData5 = new List<string>() { "C", "D", "B", "A", "E" };
            DataChunkRoute<string> chunkRoute5 = new DataChunkRoute<string>(srcData5, masterNodeList, routePrefixDictionary);
            chunkRoute5.computeFullRoute();
        }

        static void Main(string[] args)
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node<string>,List<Route<string>>> routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();

            threeThreeRoutes(masterNodeList, routePrefixDictionary);

//            twoFiveRoutes(masterNodeList, routePrefixDictionary);

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
