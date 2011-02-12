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
        private RouteFactory<string> routeFactory;
        private ValueNode<string> vn;
        private ValueNode<string> vn2;
        private ValueNode<string> vn3;
        private Edge edge;
        private Edge edge2;

        [SetUp]
        public void SetUp()
        {
            nodeList = new MasterNodeList<string>();
            vn = nodeList.newValueNodeFromValue("A");
            vn2 = nodeList.newValueNodeFromValue("B");
            vn3 = nodeList.newValueNodeFromValue("C");
            edge = new Edge() { 
                link = new DirectedPair()
                {
                    from = vn,
                    to = vn2
                }
            };
            edge2 = new Edge()
            {
                link = new DirectedPair()
                {
                    from = vn3,
                    to = vn
                }
            };
            routeFactory = new RouteFactory<string>();
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
            DataChunkRoute<string> prevRoute = routeFactory.newDataChunkRoute(
                nodeList.newStartNode());
            RouteCriterion<string> criterion = new RouteCriterion<string>() {
                desiredSequence = stringSeq, routeSoFar = prevRoute };
            IEnumerable<Route> matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(1, matchingRoutes.Count());

            criterion.desiredSequence = new List<string>() { "A", "C" };
            matchingRoutes = vn.findMatchingRoutes(criterion);
            Assert.AreEqual(0, matchingRoutes.Count());
        }
    }
}
