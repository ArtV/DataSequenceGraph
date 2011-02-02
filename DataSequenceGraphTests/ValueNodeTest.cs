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
        private ValueNode<string> vn3;
        private Edge<string> edge;
        private Edge<string> edge2;

        [SetUp]
        public void SetUp()
        {
            nodeList = new MasterNodeList<string>();
            vn = nodeList.newValueNodeFromValue("A");
            vn2 = nodeList.newValueNodeFromValue("B");
            vn3 = nodeList.newValueNodeFromValue("C");
            edge = new Edge<string>() { 
                link = new DirectedPair<string>()
                {
                    from = vn,
                    to = vn2
                },
                requisiteLink = new DirectedPair<string>()
                {
                    from = vn3,
                    to = vn
                }
            };
            edge2 = new Edge<string>()
            {
                link = new DirectedPair<string>()
                {
                    from = vn3,
                    to = vn
                }
            };
            RouteFactory<string> routeFactory = new RouteFactory<string>();
            routeFactory.newRouteFromEdge(edge);
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
            IEnumerable<Node<string>> previousNodes = new List<Node<string>>() { vn3, vn };
            RouteCriterion<string> criterion = new RouteCriterion<string>() { 
                desiredSequence = stringSeq, previousNodeSequence = previousNodes };
            IEnumerable<Route<string>> matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(1, matchingRoutes.Count());

            criterion.desiredSequence = new List<string>() { "A", "C" };
            matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(0, matchingRoutes.Count());
        }
    }
}
