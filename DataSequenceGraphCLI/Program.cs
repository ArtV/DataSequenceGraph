using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node<string>,List<Route<string>>> routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();

            List<string> srcData = new List<string>() { "A", "B", "C" };
            List<string> srcData2 = new List<string>() { "A", "A", "D" };
            List<string> srcData3 = new List<string>() { "A", "D", "E" };

            DataChunkRoute<string> chunkRoute = new DataChunkRoute<string>(srcData, masterNodeList, routePrefixDictionary);
            DataChunkRoute<string> chunkRoute2 = new DataChunkRoute<string>(srcData2, masterNodeList, routePrefixDictionary);
            DataChunkRoute<string> chunkRoute3 = new DataChunkRoute<string>(srcData3, masterNodeList, routePrefixDictionary);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();

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
                        route.connectedNodes.ElementAt(1).SequenceNumber;
                    Console.Out.WriteLine(outStr);
                }
            }
        }
    }
}
