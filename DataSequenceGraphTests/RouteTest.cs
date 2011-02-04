using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.DataChunk;

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
        private Route<string> routeBC;
        private Edge<string> edgeBC;
        private Route<string> routeCD;
        private Edge<string> edgeCD;

        private Route<string> routeABC;        

        private Route<string> routeABCD;

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
                }
            };
            edgeBC = new Edge<string>()
            {
                link = new DirectedPair<string>()
                {
                    from = nodeB,
                    to = nodeC
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
            routeBC = routeFactory.newRouteFromEdge(edgeBC);
            routeCD = routeFactory.newRouteFromEdge(edgeCD);

            routeABC = routeFactory.newRouteFromConnectedRoutes(routeAB, routeBC);

            routeABCD = routeFactory.newRouteFromConnectedRoutes(routeABC, routeCD);
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
            DataChunkRoute<string> prevRoute = new DataChunkRoute<string>(list.newStartNode(new StringDataChunk(null)));
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                routeSoFar = prevRoute 
            };
            Assert.IsTrue(routeAB.prefixMatches(criterion));

            prevRoute = new DataChunkRoute<string>(list.newStartNode(new StringDataChunk(null)));
            criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B", "C" },
                routeSoFar = prevRoute
            };
            Assert.IsTrue(routeABC.prefixMatches(criterion));
            Assert.IsTrue(routeABCD.prefixMatches(criterion));
        }

        [Test]
        public void routeFromSpec()
        {
            EdgeRouteSpec specAD = new EdgeRouteSpec() { FromNumber = 0, ToNumber = 3 };
            RouteFactory<string> factory = new RouteFactory<string>();
            Assert.Throws<InvalidOperationException>(delegate { factory.newRouteFromSpec(specAD); });
            factory.masterNodeList = list;
            EdgeRoute<string> routeAD = factory.newRouteFromSpec(specAD);
            Assert.AreSame(list.getValueNodesByValue("A").First(), routeAD.startNode);
            Assert.AreSame(list.getValueNodesByValue("D").First(), routeAD.connectedNodes.Last());

            EdgeRouteSpec specCD = (routeCD as EdgeRoute<string>).ToEdgeRouteSpec();
            Assert.AreEqual(2, specCD.FromNumber);
            Assert.AreEqual(3, specCD.ToNumber);
            Assert.AreEqual(0, specCD.RequisiteFromNumber);
            Assert.AreEqual(1, specCD.RequisiteToNumber);
        }
    }
}
