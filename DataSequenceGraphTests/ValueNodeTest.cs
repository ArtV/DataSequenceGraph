using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    public class ValueNodeTest
    {
        private MasterNodeList<string> nodeList;
        private ValueNode<string> vn;
        private ValueNode<string> vn2;

        [SetUp]
        public void SetUp()
        {
            nodeList = new MasterNodeList<string>();
            vn = nodeList.newValueNodeFromValue("A");
            vn2 = nodeList.newValueNodeFromValue("B");
            Route<string>.newRouteBetween(vn, vn2);
        }

        [Test]
        public void createValueNode()
        {
            Assert.AreEqual("A", vn.Value);
            Assert.AreEqual(0, vn.SequenceNumber);

            Assert.AreEqual("B", vn2.Value);
            Assert.AreEqual(1, vn2.SequenceNumber);
        }

        [Test]
        public void getOutgoingRoutes()
        {
            Assert.AreEqual(1,vn.OutgoingRoutes.Count());
            Assert.AreEqual(0, vn2.OutgoingRoutes.Count());
        }

        [Test]
        public void findMatchingRoutes()
        {
            IEnumerable<string> stringSeq = new List<string>() { "A","B" };
            IEnumerable<Node<string>> previousNodes = new List<StartNode<string>>();
            RouteCriterion<string> criterion = new RouteCriterion<string>() { 
                desiredSequence = stringSeq, previousNodes = previousNodes };
            IEnumerable<Route<string>> matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(1, matchingRoutes.Count());

            criterion.desiredSequence = new List<string>() { "A", "C" };
            matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(0, matchingRoutes.Count());
        }
    }
}
