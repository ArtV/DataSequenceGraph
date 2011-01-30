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

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            chunkRoute = new DataChunkRoute<string>(srcData, list);
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
            Assert.AreEqual("A", ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1)).Value);
        }

        [Test]
        public void createEndNode()
        {
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
            Assert.IsTrue(chunkRoute.Done);
            Assert.AreEqual(5, chunkRoute.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute.finishNode);
        }
    }
}
