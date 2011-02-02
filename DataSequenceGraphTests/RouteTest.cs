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

            edgeAB = new Edge<string>()
            {
                link = new DirectedPair<string>()
                {
                    from = nodeA,
                    to = nodeB
                },
                requisiteLink = new DirectedPair<string>()
                {
                    from = nodeC,
                    to = nodeD
                }
            };
            edgeCD = new Edge<string>() 
            { 
                link = new DirectedPair<string>()
                {
                    from = nodeC,
                    to = nodeD
                },
                requisiteLink = new DirectedPair<string>()
                {
                    from = nodeA,
                    to = nodeB
                }
            };
            routeAB = routeFactory.newRouteFromEdge(edgeAB);
            routeCD = routeFactory.newRouteFromEdge(edgeCD);
        }

        [Test]
        public void newRouteFromEdges()
        {
            Edge<string> newEdge = new Edge<string>() 
            {
                link = new DirectedPair<string>()
                {
                    from = nodeA,
                    to = nodeB
                },
                requisiteLink = new DirectedPair<string>()
                {
                    from = nodeC,
                    to = nodeD
                }
            };
            Route<string> route = routeFactory.newRouteFromEdge(newEdge);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void routePrefixMatches()
        {
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                previousNodeSequence = new List<Node<string>>() { nodeC, nodeD }
            };
            Assert.IsTrue(routeAB.prefixMatches(criterion));
        }
    }
}
