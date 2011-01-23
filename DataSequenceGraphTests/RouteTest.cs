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

            edgeAB = new Edge<string>() { from = nodeA, to = nodeB };
            routeAB = Route<string>.newRouteBetween(edgeAB,new List<Edge<string>>());
            edgeCD = new Edge<string>() { from = nodeC, to = nodeD };
            routeCD = Route<string>.newRouteBetween(edgeCD,new List<Edge<string>>());
        }

        [Test]
        public void newRouteFromEdges()
        {
            Edge<string> newEdge = new Edge<string>() { from = nodeA, to = nodeB };
            // the same edge wouldn't be passed twice in real situations
            Route<string> route = Route<string>.newRouteBetween(newEdge,new List<Edge<string>>() { newEdge });
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void routeMatches()
        {
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                previousEdges = new List<Edge<string>>()
            };
            Assert.IsTrue(routeAB.matches(criterion));
        }

    }
}
