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
        private MasterNodeList<string> list;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
        }

        [Test]
        public void chunkRouteInit()
        {
            List<string> srcData = new List<string>() { "A","B","C" };
            DataChunkRoute<string> route = new DataChunkRoute<string>(srcData,list);
            Assert.AreSame(srcData, route.SourceData);
            StartNode<string> firstNode = route.InitialNode;
            Assert.IsNotNull(firstNode);
        }
    }
}
