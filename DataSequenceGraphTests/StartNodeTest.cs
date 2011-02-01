using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    class StartNodeTest
    {
        [Test]
        public void startNodeInit()
        {
            MasterNodeList<string> list = new MasterNodeList<string>();
            StartNode<string> node = list.newStartNode();
            Assert.IsInstanceOf<StartNode<string>>(node);
            Assert.IsTrue(node.isStartNode);
        }
    }
}
