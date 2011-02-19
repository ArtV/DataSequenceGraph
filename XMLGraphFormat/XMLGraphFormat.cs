using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using DataSequenceGraph;

namespace DataSequenceGraph.Format
{
    public class XMLGraphFormat<NodeValType>
    {
        public NodeValueExporter<NodeValType> nodeValueExporter { get; set; }
        public NodeValueParser<NodeValType> nodeValueParser { get; set; }

        public XMLGraphFormat()
        {
            this.nodeValueExporter = new ToStringNodeValueExporter<NodeValType>();
        }

        public XMLGraphFormat(NodeValueExporter<NodeValType> nodeValueConverter)
        {
            this.nodeValueExporter = nodeValueConverter;
        }

        public XmlDocument ToXML(MasterNodeList<NodeValType> nodeList)
        {
            XmlDocument returnDoc = new XmlDocument();
            returnDoc.LoadXml("<DataSequenceGraph />");
            addNodes(returnDoc, nodeList);
            addEdges(returnDoc, nodeList);
            return returnDoc;
        }

        private void addNodes(XmlDocument doc,MasterNodeList<NodeValType> nodeList)
        {
            List<NodeSpec> nodeSpecs = nodeList.AllNodeSpecs;
            int nodeIndex;
            XmlNode rootNodesElement = doc.CreateNode(XmlNodeType.Element, "Nodes", "");
            doc.DocumentElement.AppendChild(rootNodesElement);
            XmlNode nodeElement;
            XmlAttribute IDAttr;
            XmlAttribute kindAttr;
            NodeSpec currentNodeSpec;
            ValueNodeSpec<NodeValType> valSpec;
            XmlText valueText;
            XmlAttribute valueRefAttr;
            for (nodeIndex = 0; nodeIndex <= nodeSpecs.Count - 1; nodeIndex++)
            {
                currentNodeSpec = nodeSpecs[nodeIndex];
                nodeElement = doc.CreateNode(XmlNodeType.Element, "Node", "");
                rootNodesElement.AppendChild(nodeElement);

                IDAttr = doc.CreateAttribute("ID");
                IDAttr.Value = currentNodeSpec.SequenceNumber.ToString();
                nodeElement.Attributes.Append(IDAttr);

                kindAttr = doc.CreateAttribute("Kind");
                kindAttr.Value = currentNodeSpec.kind.ToString();
                nodeElement.Attributes.Append(kindAttr);

                if (currentNodeSpec.kind == NodeKind.ValueNode)
                {
                    valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    IEnumerable<ValueNode<NodeValType>> nodesWithValue = nodeList.getValueNodesByValue(valSpec.Value);
                    int indexOfFirstValue = nodesWithValue.ElementAt(0).SequenceNumber;
                    if (nodesWithValue.Count() > 1 && currentNodeSpec.SequenceNumber != indexOfFirstValue)
                    {
                        valueRefAttr = doc.CreateAttribute("valueRef");
                        valueRefAttr.Value = indexOfFirstValue.ToString();
                        nodeElement.Attributes.Append(valueRefAttr);
                    }
                    else
                    {
                        valueText = doc.CreateTextNode(nodeValueExporter.ToNodeValueString(valSpec.Value));
                        nodeElement.AppendChild(valueText);
                    }
                }
            }
        }       

        private void addEdges(XmlDocument doc, MasterNodeList<NodeValType> nodeList)
        {
            IEnumerable<EdgeRouteSpec> routeSpecs = nodeList.AllEdgeSpecs;
            XmlNode rootEdgesElement = doc.CreateNode(XmlNodeType.Element, "Edges", "");
            doc.DocumentElement.AppendChild(rootEdgesElement);
            XmlNode edgeElement;
            XmlAttribute fromAttr;
            XmlAttribute toAttr;
            XmlAttribute reqFromAttr;
            XmlAttribute reqToAttr;
            foreach (EdgeRouteSpec spec in routeSpecs)
            {
                edgeElement = doc.CreateNode(XmlNodeType.Element, "Edge", "");
                rootEdgesElement.AppendChild(edgeElement);

                fromAttr = doc.CreateAttribute("from");
                fromAttr.Value = spec.FromNumber.ToString();
                edgeElement.Attributes.Append(fromAttr);

                toAttr = doc.CreateAttribute("to");
                toAttr.Value = spec.ToNumber.ToString();
                edgeElement.Attributes.Append(toAttr);

                reqFromAttr = doc.CreateAttribute("fromRequisite");
                reqFromAttr.Value = spec.RequisiteFromNumber.ToString();
                edgeElement.Attributes.Append(reqFromAttr);

                reqToAttr = doc.CreateAttribute("toRequisite");
                reqToAttr.Value = spec.RequisiteToNumber.ToString();
                edgeElement.Attributes.Append(reqToAttr);
            }
        }

        public MasterNodeList<NodeValType> ToNodeList(XmlReader reader)
        {
            if (nodeValueParser == null)
            {
                throw new InvalidOperationException("nodeValueParser must be set to recreate a graph from XML");
            }
            XPathDocument doc = new XPathDocument(reader);
            XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("DataSequenceGraph");
            rootNav.MoveToFirstChild();            
            MasterNodeList<NodeValType> nodeList = new MasterNodeList<NodeValType>();
            IEnumerable<NodeSpec> nodeSpecs = handleNodes(rootNav);
            rootNav.MoveToNext();
            IEnumerable<EdgeRouteSpec> edgeSpecs = handleEdges(rootNav);
            nodeList.reloadNodesThenRoutesFromSpecs(nodeSpecs, edgeSpecs);
            return nodeList;
        }

        private List<NodeSpec> handleNodes(XPathNavigator nodesRootNav)
        {
            XPathNodeIterator nodes = nodesRootNav.SelectChildren("Node", "");
            List<NodeSpec> newNodeSpecs = new List<NodeSpec>();
            NodeKind curKind;
            int sequenceNumber;
            NodeValType nodeValue;
            ValueNodeSpec<NodeValType> valueRefNode;
            foreach (XPathNavigator nodeNav in nodes)
            {
                nodeNav.MoveToAttribute("ID","");
                sequenceNumber = nodeNav.ValueAsInt;
                nodeNav.MoveToParent();
                nodeNav.MoveToAttribute("Kind", "");
                curKind = (NodeKind)System.Enum.Parse(typeof(NodeKind), nodeNav.Value);
                if (curKind == NodeKind.ValueNode)
                {
                    nodeNav.MoveToParent();
                    if (nodeNav.MoveToAttribute("valueRef", ""))
                    {
                        valueRefNode = newNodeSpecs.OfType<ValueNodeSpec<NodeValType>>().First(
                            spec => spec.SequenceNumber == nodeNav.ValueAsInt);
                        nodeValue = valueRefNode.Value;
                    }
                    else
                    {
                        nodeValue = nodeValueParser.parseToValue(nodeNav.Value);
                    }
                    newNodeSpecs.Add(new ValueNodeSpec<NodeValType>()
                    {
                        kind = curKind,
                        Value = nodeValue,
                        SequenceNumber = sequenceNumber
                    });
                }
                else
                {
                    newNodeSpecs.Add(new NodeSpec()
                    {
                        kind = (NodeKind)System.Enum.Parse(typeof(NodeKind), nodeNav.Value),
                        SequenceNumber = sequenceNumber
                    });
                }
            }
            return newNodeSpecs;
        }

        private IEnumerable<EdgeRouteSpec> handleEdges(XPathNavigator edgesRootNav)
        {
            XPathNodeIterator edges = edgesRootNav.SelectChildren("Edge", "");
            List<EdgeRouteSpec> newNodeSpecs = new List<EdgeRouteSpec>();
            foreach (XPathNavigator edgeNav in edges)
            {
                newNodeSpecs.Add(new EdgeRouteSpec()
                {
                    FromNumber = Convert.ToInt32(edgeNav.Evaluate("number(@from)")),
                    ToNumber = Convert.ToInt32(edgeNav.Evaluate("number(@to)")),
                    RequisiteFromNumber = Convert.ToInt32(edgeNav.Evaluate("number(@fromRequisite)")),
                    RequisiteToNumber = Convert.ToInt32(edgeNav.Evaluate("number(@toRequisite)"))
                });
            }
            return newNodeSpecs;
        }
    }
}
