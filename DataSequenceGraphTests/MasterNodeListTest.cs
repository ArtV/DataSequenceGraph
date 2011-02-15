using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    class MasterNodeListTest
    {
        private MasterNodeList<string> list;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            list.newValueNodeFromValue("A");
            list.newValueNodeFromValue("B");
        }

        [Test]
        public void getNodeBySequenceNumber()
        {
            ValueNode<string> nodeB = (ValueNode<string>) list.nodeByNumber(1);            
            Assert.AreEqual("B", nodeB.Value);
        }

        [Test]
        public void getValueNodesByValue()
        {
            IEnumerable<ValueNode<string>> matchedNodes = list.getValueNodesByValue("A");
            Assert.AreEqual(1, matchedNodes.Count());
            Assert.AreEqual("A", matchedNodes.ElementAt(0).Value);
        }

        private MasterNodeList<string> threeSixNodeList()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            DataChunkRouteBlazerTest.threeSixChunks(nodeList,
                new Dictionary<Node, List<Route>>());
            return nodeList;
        }

        [Test]
        public void produceDataChunksFromList()
        {
            MasterNodeList<string> nodeList = threeSixNodeList();
            foreach (var node in nodeList.AllNodes)
            {
                Assert.GreaterOrEqual(node.OutgoingRoutes.Count(), 0);
            }

            IEnumerable<IEnumerable<string>> chunks = nodeList.enumerateDataChunks();
            Assert.AreEqual("A", chunks.ElementAt(0).ElementAt(0));
            Assert.AreEqual("M", chunks.ElementAt(2).ElementAt(3));
        }

        [Test]
        public void getAndSetNodeSpecs()
        {
            MasterNodeList<string> nodeListExporter = threeSixNodeList();
            MasterNodeList<string> nodeListImporter = new MasterNodeList<string>();
            nodeListImporter.reloadNodesFromSpecs(nodeListExporter.AllNodeSpecs);
            Assert.AreEqual(nodeListExporter.AllNodes.Count(), nodeListImporter.AllNodes.Count());
            Assert.AreEqual(nodeListExporter.AllNodes.OfType<GateNode>().Count(),
                nodeListImporter.AllNodes.OfType<GateNode>().Count());
            Assert.AreEqual(nodeListExporter.AllNodes.OfType<ValueNode<string>>().ElementAt(5).Value,
                nodeListImporter.AllNodes.OfType<ValueNode<string>>().ElementAt(5).Value);
        }
    }
}
