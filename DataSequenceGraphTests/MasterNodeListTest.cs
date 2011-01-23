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
        public void getValueNodes()
        {
            IEnumerable<ValueNode<string>> nodeList = list.getValueNodes();
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
    }
}
