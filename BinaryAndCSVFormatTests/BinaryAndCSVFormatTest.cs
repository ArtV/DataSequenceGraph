using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph;
using DataSequenceGraph.Format;

namespace BinaryAndCSVFormatTests
{
    [TestFixture]
    public class BinaryAndCSVFormatTest
    {
        [Test]
        public void threeThreeTest()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> prefixD = new Dictionary<Node, List<Route>>();
            DataChunkRouteBlazerTest.threeThreeChunks(nodeList, prefixD);
            int origCount = nodeList.AllNodes.Count();
            int origEdgeCount = nodeList.AllEdgeSpecs.Count();

            BinaryAndCSVFormat<string> format = new BinaryAndCSVFormat<string>("nodesEdges.dat", "values.csv");
            format.nodeValueParser = new StringNodeValueParser();
            format.ToBinaryAndCSV(nodeList);

            nodeList = new MasterNodeList<string>();
            prefixD = new Dictionary<Node, List<Route>>();
            nodeList = format.ToNodeList();
            Assert.AreEqual(origCount, nodeList.AllNodes.Count());
            Assert.AreEqual(origEdgeCount, nodeList.AllEdgeSpecs.Count());
        }
    }
}
