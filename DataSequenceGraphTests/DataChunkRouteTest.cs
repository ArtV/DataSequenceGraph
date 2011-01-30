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
        private Dictionary<IEnumerable<string>, Route<string>> routePrefixDictionary;

        List<string> srcData2 = new List<string>() { "A", "A", "A" };
        private DataChunkRoute<string> chunkRoute2;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            routePrefixDictionary = new Dictionary<IEnumerable<string>, Route<string>>();
            chunkRoute = new DataChunkRoute<string>(srcData, list,routePrefixDictionary);

            chunkRoute2 = new DataChunkRoute<string>(srcData2, list, routePrefixDictionary);
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
        }

        [Test]
        public void createEndNode()
        {
            chunkRouteAppendThrice();
            Assert.IsTrue(chunkRoute.Done);
            Assert.AreEqual(5, chunkRoute.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute.finishNode);
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
    }
}
