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

        [SetUp]
        public void SetUp()
        {
            nodeList = new MasterNodeList<string>();
        }

        [Test]
        public void createValueNode()
        {
            ValueNode<string> vn = nodeList.newValueNodeFromValue("A");
            Assert.AreEqual("A", vn.Value);
            Assert.AreEqual(0, vn.SequenceNumber);

            vn = nodeList.newValueNodeFromValue("B");
            Assert.AreEqual("B", vn.Value);
            Assert.AreEqual(1, vn.SequenceNumber);
        }
    }
}
