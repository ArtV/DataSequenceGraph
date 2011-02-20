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
            Assert.AreEqual(XMLGraphFormat<int>.ROOTELEM, emptyDoc.DocumentElement.LocalName);
        }

        [Test]
        public void threeSix()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> prefixD = new Dictionary<Node, List<Route>>();
            DataChunkRouteBlazerTest.threeSixChunks(nodeList, prefixD);
            XmlDocument threeSixDoc = new XMLGraphFormat<string>().ToXML(nodeList);
            XmlNode nodesElement = threeSixDoc.DocumentElement.ChildNodes[0];
            Assert.AreEqual(XMLGraphFormat<int>.NODESROOTELEM, nodesElement.LocalName);
            XmlNodeList nodeElements = nodesElement.ChildNodes;

            XmlNode firstStartNode = nodeElements[0];
            Assert.AreEqual(XMLGraphFormat<int>.NODEELEM, firstStartNode.LocalName);
            Assert.AreEqual("0", firstStartNode.Attributes[XMLGraphFormat<int>.SEQNUMATTR].Value);
            Assert.AreEqual(NodeKind.GateNode.ToString(), firstStartNode.Attributes[XMLGraphFormat<int>.NODEKINDATTR].Value);

            XmlNode firstValueNode = nodeElements[3];
            Assert.AreEqual("3", firstValueNode.Attributes[XMLGraphFormat<int>.SEQNUMATTR].Value);
            Assert.AreEqual(NodeKind.ValueNode.ToString(), firstValueNode.Attributes[XMLGraphFormat<int>.NODEKINDATTR].Value);
            Assert.AreEqual("A", firstValueNode.InnerText);

            XmlNode edgesElement = threeSixDoc.DocumentElement.ChildNodes[1];
            Assert.AreEqual(XMLGraphFormat<int>.EDGESROOTELEM, edgesElement.LocalName);
            XmlNodeList edgeElements = edgesElement.ChildNodes;

            XmlNode firstEdge = edgeElements[0];
            Assert.AreEqual(XMLGraphFormat<int>.EDGEELEM, firstEdge.LocalName);
            Assert.AreEqual("0", firstEdge.Attributes[XMLGraphFormat<int>.FROMATTR].Value);
            Assert.AreEqual("3", firstEdge.Attributes[XMLGraphFormat<int>.TOATTR].Value);
            Assert.AreEqual("-1", firstEdge.Attributes[XMLGraphFormat<int>.REQFROMATTR].Value);
            Assert.AreEqual("-1", firstEdge.Attributes[XMLGraphFormat<int>.REQTOATTR].Value);
        }

        [Test]
        public void threeSixFromXMLToList()
        {
            XMLGraphFormat<string> format = new XMLGraphFormat<string>();
            format.nodeValueParser = new StringNodeValueParser();
            XmlDocument sampleDoc = new XmlDocument();
            string myDocTempl =
            @"<{0}><{1}>
            <{2} {3}=""0"" {4}=""GateNode"" />
            <{2} {3}=""1"" {4}=""ValueNode"">Here</{2}>            
            </{1}><{5}>
            <{6} {7}=""0"" {8}=""1"" {9}=""-1"" {10}=""-1"" />
            <{6} {7}=""1"" {8}=""0"" {9}=""-1"" {10}=""-1"" />
            </{5}></{0}>";
            string myDoc = String.Format(myDocTempl, XMLGraphFormat<int>.ROOTELEM,
                XMLGraphFormat<uint>.NODESROOTELEM, XMLGraphFormat<bool>.NODEELEM,
                XMLGraphFormat<long>.SEQNUMATTR, XMLGraphFormat<byte>.NODEKINDATTR,
                XMLGraphFormat<short>.EDGESROOTELEM, XMLGraphFormat<sbyte>.EDGEELEM,
                XMLGraphFormat<float>.FROMATTR, XMLGraphFormat<double>.TOATTR,
                XMLGraphFormat<char>.REQFROMATTR, XMLGraphFormat<ushort>.REQTOATTR);
            MasterNodeList<string> nodeList = format.ToNodeList(XmlReader.Create(new StringReader(myDoc)));
            Assert.AreEqual(2, nodeList.AllNodes.Count());
            Assert.IsInstanceOf<ValueNode<string>>(nodeList.AllNodes.ElementAt(1));
            Assert.AreEqual(1, nodeList.getValueNodesByValue("Here").Count());
        }

        [Test]
        public void reuseNodeValueByRef()
        {
            MasterNodeList<string> nodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> prefixD = new Dictionary<Node, List<Route>>();
            DataChunkRouteBlazerTest.threeThreeChunks(nodeList, prefixD);
            XmlDocument threeThreeDoc = new XMLGraphFormat<string>().ToXML(nodeList);
            XmlNode nodesElement = threeThreeDoc.DocumentElement.ChildNodes[0];
            XmlNodeList nodeElements = nodesElement.ChildNodes;

            int firstAIndex = nodeList.getValueNodesByValue("A").ElementAt(0).SequenceNumber;
            int secondAIndex = nodeList.getValueNodesByValue("A").ElementAt(1).SequenceNumber;
            XmlNode secondAXmlElem = nodeElements[secondAIndex];
            Assert.AreEqual(firstAIndex.ToString(), secondAXmlElem.Attributes[XMLGraphFormat<ushort>.VALUEREFATTR].Value);
            Assert.AreEqual("", secondAXmlElem.InnerText);
        }
    }
}
