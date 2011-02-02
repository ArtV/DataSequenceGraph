using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    class DataChunkRouteTest
    {
        List<string> srcData = new List<string>() { "A", "B", "C" };
        private MasterNodeList<string> list;
        private DataChunkRoute<string> chunkRoute;
        private Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary;

        List<string> srcData2 = new List<string>() { "A", "A", "D" };
        private DataChunkRoute<string> chunkRoute2;

        List<string> srcData3 = new List<string>() { "A", "D", "E" };
        private DataChunkRoute<string> chunkRoute3;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();
            chunkRoute = new DataChunkRoute<string>(srcData, list,routePrefixDictionary);

            chunkRoute2 = new DataChunkRoute<string>(srcData2, list, routePrefixDictionary);

            chunkRoute3 = new DataChunkRoute<string>(srcData3, list, routePrefixDictionary);
        }

        [Test]
        public void chunkRouteInit()
        {
            Assert.AreSame(srcData, chunkRoute.SourceData);
            StartNode<string> firstNode = chunkRoute.InitialNode;
            Assert.IsNotNull(firstNode);
        }

        [Test]
        public void createMissingValueNode()
        {
            chunkRoute.appendToRoute();
            Assert.AreEqual(2, chunkRoute.connectedNodes.Count());
            ValueNode<string> Anode = ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1));
            Assert.AreEqual("A", Anode.Value);
            Route<string> startToA = chunkRoute.InitialNode.OutgoingRoutes.ElementAt(0);
            Assert.AreSame(Anode, startToA.connectedNodes.Last());
            Assert.AreEqual(1, chunkRoute.InitialNode.OutgoingRoutes.Count());
        }

        [Test]
        public void createEndNode()
        {
            chunkRouteAppendThrice();
            Assert.IsTrue(chunkRoute.Done);
            Assert.AreEqual(5, chunkRoute.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute.finishNode);
            Route<string> routeToEnd = chunkRoute.connectedNodes.ElementAt(3).OutgoingRoutes.ElementAt(0);
            Assert.AreSame(chunkRoute.finishNode, routeToEnd.connectedNodes.Last());

            Assert.IsFalse(chunkRoute2.Done);
            chunkRoute2.computeFullRoute();
            Assert.IsTrue(chunkRoute2.Done);
            Assert.AreEqual(5, chunkRoute2.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute2.finishNode);
        }

        [Test]
        public void sameChunkNotReusesValueNode()
        {
            chunkRoute2.appendToRoute();
            chunkRoute2.appendToRoute();
            ValueNode<string> AnodeFirst = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(1));
            ValueNode<string> AnodeSecond = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(2));
            Assert.AreNotSame(AnodeSecond, AnodeFirst);
        }

        private void chunkRouteAppendThrice()
        {
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
        }

        [Test]
        public void differentChunkReusesValueNode()
        {
            chunkRouteAppendThrice();
            chunkRoute2.appendToRoute();
            ValueNode<string> Anode = ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1));
            ValueNode<string> otherChunkNode = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(1));
            Assert.AreSame(Anode, otherChunkNode);
        }

        [Test]
        public void choosesLongerExistingRoute()
        {
            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();

            ValueNode<string> AnodeOne = ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1));
            ValueNode<string> AnodeTwo = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(2));
            ValueNode<string> route3ANode = ((ValueNode<string>)chunkRoute3.connectedNodes.ElementAt(1));

            Assert.AreNotSame(AnodeOne, route3ANode);
            Assert.AreSame(AnodeTwo, route3ANode);
        }
    }
}
