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
        private Route<string> routeCD;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            nodeA = list.newValueNodeFromValue("A");
            nodeB = list.newValueNodeFromValue("B");
            nodeC = list.newValueNodeFromValue("C");
            nodeD = list.newValueNodeFromValue("D");

            routeAB = Route<string>.newRouteBetween(nodeA, nodeB);
            routeCD = Route<string>.newRouteBetween(nodeC, nodeD);
        }

        [Test]
        public void newRouteFromNodes()
        {
            Route<string> route = Route<string>.newRouteBetween(nodeA, nodeB);
            Assert.AreSame(nodeA, route.startNode);
            Assert.AreSame(nodeB, route.connectedNodes.ElementAt(1));
        }

        [Test]
        public void routeMatches()
        {
            RouteCriterion<string> criterion = new RouteCriterion<string>()
            {
                desiredSequence = new List<string>() { "A", "B" },
                previousNodes = new List<ValueNode<string>>()
            };
            Assert.IsTrue(routeAB.matches(criterion));
        }

    }
}
