using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    public class ValueNodeTest
    {
        private MasterNodeList<string> nodeList;
        private ValueNode<string> vn;
        private ValueNode<string> vn2;

        [SetUp]
        public void SetUp()
        {
            nodeList = new MasterNodeList<string>();
            vn = nodeList.newValueNodeFromValue("A");
            vn2 = nodeList.newValueNodeFromValue("B");
            Route.newRouteBetween(vn, vn2);
        }

        [Test]
        public void createValueNode()
        {
            Assert.AreEqual("A", vn.Value);
            Assert.AreEqual(0, vn.SequenceNumber);

            Assert.AreEqual("B", vn2.Value);
            Assert.AreEqual(1, vn2.SequenceNumber);
        }

        [Test]
        public void getOutgoingRoutes()
        {
            Assert.AreEqual(1,vn.OutgoingRoutes.Count());
            Assert.AreEqual(0, vn2.OutgoingRoutes.Count());
        }
    }
}
