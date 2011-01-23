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

        private Route routeAB;
        private Route routeCD;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            nodeA = list.newValueNodeFromValue("A");
            nodeB = list.newValueNodeFromValue("B");
            nodeC = list.newValueNodeFromValue("C");
            nodeD = list.newValueNodeFromValue("D");

            routeAB = Route.newRouteBetween(nodeA, nodeB);
            routeCD = Route.newRouteBetween(nodeC, nodeD);
        }

        [Test]
        public void newRouteFromNodes()
        {
            Route route = Route.newRouteBetween(nodeA, nodeB);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void newRouteFromRoutes()
        {
            Route route = Route.connectRoutes(routeAB, routeCD);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeD, route.connectedNodes.ElementAt(3));
        }
    }
}
