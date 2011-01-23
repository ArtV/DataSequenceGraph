using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    public class EdgeTest
    {
        private MasterNodeList<string> list;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
        }

        [Test]
        public void nodeSequenceToEdges()
        {
            IEnumerable<Node<string>> nodeSeq = new List<Node<string>>()
            {
                list.newValueNodeFromValue("A"),
                list.newValueNodeFromValue("B"),
                list.newValueNodeFromValue("C"),
                list.newValueNodeFromValue("D")
            };
            IEnumerable<Edge<string>> edgeSeq = Edge<string>.nodeSequenceToEdges(nodeSeq);
            Assert.AreEqual(3, edgeSeq.Count());
            Assert.AreEqual("A", ((ValueNode<string>)edgeSeq.ElementAt(0).from).Value);
            Assert.AreEqual("B",((ValueNode<string>) edgeSeq.ElementAt(1).from).Value);
            Assert.AreEqual("C", ((ValueNode<string>)edgeSeq.ElementAt(2).from).Value);
        }
    }
}
