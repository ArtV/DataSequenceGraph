using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using DataSequenceGraph.Format;
using NUnit.Framework;
using System.Xml;
using System.IO;

namespace XMLGraphFormatTests
{
    [TestFixture]
    public class XMLGraphFormatTest
    {
        [Test]
        public void testEmptyNodeList()
        {
            MasterNodeList<int> nodeList = new MasterNodeList<int>();
            XmlDocument emptyDoc = new XMLGraphFormat<int>().ToXML(nodeList);
            Assert.AreEqual("DataSequenceGraph", emptyDoc.DocumentElement.LocalName);
        }

        [Test]
        public void threeSix()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> prefixD = new Dictionary<Node, List<Route>>();
            DataChunkRouteBlazerTest.threeSixChunks(nodeList, prefixD);
            XmlDocument threeSixDoc = new XMLGraphFormat<string>().ToXML(nodeList);
            XmlNode nodesElement = threeSixDoc.DocumentElement.ChildNodes[0];
            Assert.AreEqual("Nodes", nodesElement.LocalName);
            XmlNodeList nodeElements = nodesElement.ChildNodes;

            XmlNode firstStartNode = nodeElements[0];
            Assert.AreEqual("Node", firstStartNode.LocalName);
            Assert.AreEqual("0", firstStartNode.Attributes["ID"].Value);
            Assert.AreEqual(NodeKind.GateNode.ToString(), firstStartNode.Attributes["Kind"].Value);

            XmlNode firstValueNode = nodeElements[3];
            Assert.AreEqual("3", firstValueNode.Attributes["ID"].Value);
            Assert.AreEqual(NodeKind.ValueNode.ToString(), firstValueNode.Attributes["Kind"].Value);
            Assert.AreEqual("A", firstValueNode.InnerText);

            XmlNode edgesElement = threeSixDoc.DocumentElement.ChildNodes[1];
            Assert.AreEqual("Edges", edgesElement.LocalName);
            XmlNodeList edgeElements = edgesElement.ChildNodes;

            XmlNode firstEdge = edgeElements[0];
            Assert.AreEqual("Edge", firstEdge.LocalName);
            Assert.AreEqual("0", firstEdge.Attributes["from"].Value);
            Assert.AreEqual("3", firstEdge.Attributes["to"].Value);
            Assert.AreEqual("-1", firstEdge.Attributes["fromRequisite"].Value);
            Assert.AreEqual("-1", firstEdge.Attributes["toRequisite"].Value);
        }

        [Test]
        public void threeSixFromXMLToList()
        {
            XMLGraphFormat<string> format = new XMLGraphFormat<string>();
            format.nodeValueParser = new StringNodeValueParser();
            XmlDocument sampleDoc = new XmlDocument();
            string myDoc =
            @"<DataSequenceGraph><Nodes>
            <Node ID=""0"" Kind=""GateNode"" />
            <Node ID=""1"" Kind=""ValueNode"">Here</Node>            
            </Nodes><Edges>
            <Edge from=""0"" to=""1"" fromRequisite=""-1"" toRequisite=""-1"" />
            <Edge from=""1"" to=""0"" fromRequisite=""-1"" toRequisite=""-1"" />
            </Edges></DataSequenceGraph>";
            MasterNodeList<string> nodeList = format.ToNodeList(XmlReader.Create(new StringReader(myDoc)));
            Assert.AreEqual(2, nodeList.AllNodes.Count());
            Assert.IsInstanceOf<ValueNode<string>>(nodeList.AllNodes.ElementAt(1));
            Assert.AreEqual(1, nodeList.getValueNodesByValue("Here").Count());
        }

    }
}
