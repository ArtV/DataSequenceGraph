using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph;
using DataSequenceGraph.Format;

namespace BinaryAndTXTFormatTests
{
    [TestFixture]
    public class BinaryAndTXTFormatTest
    {
        [Test]
        public void threeThreeTest()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> prefixD = new Dictionary<Node, List<Route>>();
            DataChunkRouteBlazerTest.threeThreeChunks(nodeList, prefixD);
            int origCount = nodeList.AllNodes.Count();
            int origEdgeCount = nodeList.AllEdgeSpecs.Count();
            Assert.AreEqual(3, nodeList.enumerateDataChunkRoutes().Count());

            BinaryAndTXTFormat<string> format = new BinaryAndTXTFormat<string>("nodesEdges.dat", "values.csv");
            format.nodeValueParser = new StringNodeValueParser();
            format.ToBinaryAndTXTFiles(nodeList);

            nodeList = new MasterNodeList<string>();
            prefixD = new Dictionary<Node, List<Route>>();
            nodeList = format.ToNodeListFromFiles();
            Assert.AreEqual(origCount, nodeList.AllNodes.Count());
            Assert.AreEqual(3, nodeList.enumerateDataChunkRoutes().Count());
            Assert.AreEqual(origEdgeCount, nodeList.AllEdgeSpecs.Count());
        }
    }
}
