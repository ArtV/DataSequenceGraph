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
        private ValueNode<string> nodeD2;
        private ValueNode<string> nodeD3;

        private Route routeAB;
        private Edge edgeAB;
        private Route routeBC;
        private Edge edgeBC;
        private Route routeCD;
        private Edge edgeCD;
        private Edge edgeCB;
        private Route routeCB;

        private Edge edgeBD;
        private Route routeBD;
        private Edge edgeBD2;
        private Route routeBD2;

        private Route routeABC;        

        private Route routeABCD;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            nodeA = list.newValueNodeFromValue("A");
            nodeB = list.newValueNodeFromValue("B");
            nodeC = list.newValueNodeFromValue("C");
            nodeD = list.newValueNodeFromValue("D");
            nodeD2 = list.newValueNodeFromValue("D");
            nodeD3 = list.newValueNodeFromValue("D");

            routeFactory = new RouteFactory<string>();

            edgeAB = new Edge()
            {
                link = new DirectedPair()
                {
                    from = nodeA,
                    to = nodeB
                }
            };
            edgeBC = new Edge()
            {
                link = new DirectedPair()
                {
                    from = nodeB,
                    to = nodeC
                }
            };
            edgeCD = new Edge() 
            { 
                link = new DirectedPair()
                {
                    from = nodeC,
                    to = nodeD
                },
                requisiteLink = new DirectedPair()
                {
                    from = nodeA,
                    to = nodeB
                }
            };
            edgeBD = new Edge()
            {
                link = new DirectedPair()
                {
                    from = nodeB,
                    to = nodeD
                },
                requisiteLink = new DirectedPair()
                {
                    from = nodeA,
                    to = nodeB
                }
            };
            edgeBD2 = new Edge()
            {
                link = new DirectedPair()
                {
                    from = nodeB,
                    to = nodeD2
                },
                requisiteLink = new DirectedPair()
                {
                    from = nodeC,
                    to = nodeB
                }
            };

            edgeCB = new Edge()
            {
                link = new DirectedPair()
                {
                    from = nodeC,
                    to = nodeB
                }
            };
            routeCB = routeFactory.newRouteFromEdge(edgeCB);
            routeAB = routeFactory.newRouteFromEdge(edgeAB);
            routeBC = routeFactory.newRouteFromEdge(edgeBC);
            routeCD = routeFactory.newRouteFromEdge(edgeCD);

            routeBD = routeFactory.newRouteFromEdge(edgeBD);
            routeBD2 = routeFactory.newRouteFromEdge(edgeBD2);

            routeABC = routeFactory.newRouteFromConnectedRoutes(routeAB, routeBC);

            routeABCD = routeFactory.newRouteFromConnectedRoutes(routeABC, routeCD);
        }

        [Test]
        public void newRouteFromRoutes()
        {
            Assert.AreEqual(3, routeABC.connectedNodes.Count);
            Assert.AreEqual(0, routeABCD.requisiteLinks.Count());
        }

        [Test]
        public void newRouteFromEdges()
        {
            Edge newEdge = new Edge() 
            {
                link = new DirectedPair()
                {
                    from = nodeA,
                    to = nodeB
                },
                requisiteLink = new DirectedPair()
                {
                    from = nodeC,
                    to = nodeD
                }
            };
            Route route = routeFactory.newRouteFromEdge(newEdge);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void routePrefixMatches()
        {
            DataChunkRoute<string> prevRoute = routeFactory.newDataChunkRoute(list.newGateNode());
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                routeSoFar = prevRoute 
            };
            Assert.IsTrue(routeAB.prefixMatches(criterion));

            prevRoute = routeFactory.newDataChunkRoute(list.newGateNode());
            criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B", "C" },
                routeSoFar = prevRoute
            };
            Assert.IsTrue(routeABC.prefixMatches(criterion));
            Assert.IsTrue(routeABCD.prefixMatches(criterion));
            
            criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "B", "D" },
                routeSoFar = routeAB
            };
            Assert.IsTrue(routeBD.prefixMatches(criterion));
            Assert.IsTrue(routeAB.meetsRequisites(routeBD.requisiteLinks));
        }

        [Test]
        public void routeFromSpec()
        {
            EdgeRouteSpec specAD = new EdgeRouteSpec() { FromNumber = 0, ToNumber = 3 };
            RouteFactory<string> factory = new RouteFactory<string>();
            Assert.Throws<InvalidOperationException>(delegate { factory.newRouteFromSpec(specAD); });
            factory.masterNodeList = list;
            EdgeRoute routeAD = factory.newRouteFromSpec(specAD);
            Assert.AreSame(list.getValueNodesByValue("A").First(), routeAD.startNode);
            Assert.AreSame(list.getValueNodesByValue("D").First(), routeAD.connectedNodes.Last());

            EdgeRouteSpec specCD = (routeCD as EdgeRoute).ToEdgeRouteSpec();
            Assert.AreEqual(2, specCD.FromNumber);
            Assert.AreEqual(3, specCD.ToNumber);
            Assert.AreEqual(0, specCD.RequisiteFromNumber);
        }
    }
}
