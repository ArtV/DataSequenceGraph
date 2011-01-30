using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    class RouteTest
    {
        private MasterNodeList<string> list;

        private RouteFactory<string> routeFactory;

        private ValueNode<string> nodeA;
        private ValueNode<string> nodeB;
        private ValueNode<string> nodeC;
        private ValueNode<string> nodeD;

        private Route<string> routeAB;
        private Edge<string> edgeAB;
        private Route<string> routeCD;
        private Edge<string> edgeCD;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            nodeA = list.newValueNodeFromValue("A");
            nodeB = list.newValueNodeFromValue("B");
            nodeC = list.newValueNodeFromValue("C");
            nodeD = list.newValueNodeFromValue("D");

            routeFactory = new RouteFactory<string>();

            edgeAB = new Edge<string>() { from = nodeA, to = nodeB };
            edgeCD = new Edge<string>() { from = nodeC, to = nodeD };
            routeAB = routeFactory.newRouteBetween(edgeAB,edgeCD);
            routeCD = routeFactory.newRouteBetween(edgeCD,edgeAB);
        }

        [Test]
        public void newRouteFromEdges()
        {
            Edge<string> newEdge = new Edge<string>() { from = nodeA, to = nodeB };
            Route<string> route = routeFactory.newRouteBetween(newEdge,edgeCD);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void routePrefixMatches()
        {
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                previousEdges = new List<Edge<string>>()
            };
            Assert.IsTrue(routeAB.prefixMatches(criterion));
        }
    }
}
