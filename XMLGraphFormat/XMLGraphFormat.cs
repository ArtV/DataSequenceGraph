﻿using System;
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
        public const string ROOTELEM = "DataSequenceGraph";
        public const string NODESROOTELEM = "Nodes";
        public const string NODEELEM = "Node";
        public const string SEQNUMATTR = "ID";
        public const string NODEKINDATTR = "Kind";
        public const string VALUEREFATTR = "ValRef";

        public const string EDGESROOTELEM = "Edges";
        public const string EDGEELEM = "Edge";
        public const string FROMATTR = "From";
        public const string TOATTR = "To";
        public const string REQFROMATTR = "ReqFrom";
        public const string REQTOATTR = "ReqTo";

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

        public XmlDocument ToXML(MasterNodeList<NodeValType> nodeList, IList<NodeSpec> nodes, IList<EdgeRouteSpec> edges)
        {
            XmlDocument returnDoc = new XmlDocument();
            returnDoc.LoadXml("<" + ROOTELEM + " />");
            addNodes(returnDoc,nodeList,nodes);
            addEdges(returnDoc,edges);
            return returnDoc;
        }

        public XmlDocument ToXML(MasterNodeList<NodeValType> nodeList)
        {
            return ToXML(nodeList, nodeList.AllNodeSpecs, nodeList.AllEdgeSpecs.ToList());
        }

        private void addNodes(XmlDocument doc,MasterNodeList<NodeValType> nodeList,IList<NodeSpec> nodeSpecs)
        {
            int nodeIndex;
            XmlNode rootNodesElement = doc.CreateNode(XmlNodeType.Element, NODESROOTELEM, "");
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
                nodeElement = doc.CreateNode(XmlNodeType.Element, NODEELEM, "");
                rootNodesElement.AppendChild(nodeElement);

                IDAttr = doc.CreateAttribute(SEQNUMATTR);
                IDAttr.Value = currentNodeSpec.SequenceNumber.ToString();
                nodeElement.Attributes.Append(IDAttr);

                kindAttr = doc.CreateAttribute(NODEKINDATTR);
                kindAttr.Value = currentNodeSpec.kind.ToString();
                nodeElement.Attributes.Append(kindAttr);

                if (currentNodeSpec.kind == NodeKind.ValueNode)
                {
                    valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    IEnumerable<int> nodeIndexesWithValue =
                        nodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);                        
                    if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.SequenceNumber != nodeIndexesWithValue.First())
                    {
                        valueRefAttr = doc.CreateAttribute(VALUEREFATTR);
                        valueRefAttr.Value = nodeIndexesWithValue.First().ToString();
                        nodeElement.Attributes.Append(valueRefAttr);
                    }
                    else
                    {
                        nodeIndexesWithValue = nodeSpecs.OfType<ValueNodeSpec<NodeValType>>().Where(
                            spec => spec.SequenceNumber < valSpec.SequenceNumber &&
                                    spec.Value.Equals(valSpec.Value)).Select(spec => spec.SequenceNumber);
                        if (nodeIndexesWithValue.Count() > 0 && valSpec.SequenceNumber != nodeIndexesWithValue.First())
                        {
                            valueRefAttr = doc.CreateAttribute(VALUEREFATTR);
                            valueRefAttr.Value = nodeIndexesWithValue.First().ToString();
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
        }       

        private void addEdges(XmlDocument doc,IEnumerable<EdgeRouteSpec> routeSpecs)
        {
            XmlNode rootEdgesElement = doc.CreateNode(XmlNodeType.Element, EDGESROOTELEM, "");
            doc.DocumentElement.AppendChild(rootEdgesElement);
            XmlNode edgeElement;
            XmlAttribute fromAttr;
            XmlAttribute toAttr;
            XmlAttribute reqFromAttr;
            XmlAttribute reqToAttr;
            foreach (EdgeRouteSpec spec in routeSpecs)
            {
                edgeElement = doc.CreateNode(XmlNodeType.Element, EDGEELEM, "");
                rootEdgesElement.AppendChild(edgeElement);

                fromAttr = doc.CreateAttribute(FROMATTR);
                fromAttr.Value = spec.FromNumber.ToString();
                edgeElement.Attributes.Append(fromAttr);

                toAttr = doc.CreateAttribute(TOATTR);
                toAttr.Value = spec.ToNumber.ToString();
                edgeElement.Attributes.Append(toAttr);

                reqFromAttr = doc.CreateAttribute(REQFROMATTR);
                reqFromAttr.Value = spec.RequisiteFromNumber.ToString();
                edgeElement.Attributes.Append(reqFromAttr);

                reqToAttr = doc.CreateAttribute(REQTOATTR);
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
            XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode(ROOTELEM);
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
            XPathNodeIterator nodes = nodesRootNav.SelectChildren(NODEELEM, "");
            List<NodeSpec> newNodeSpecs = new List<NodeSpec>();
            NodeKind curKind;
            int sequenceNumber;
            NodeValType nodeValue;
            ValueNodeSpec<NodeValType> valueRefNode;
            foreach (XPathNavigator nodeNav in nodes)
            {
                nodeNav.MoveToAttribute(SEQNUMATTR,"");
                sequenceNumber = nodeNav.ValueAsInt;
                nodeNav.MoveToParent();
                nodeNav.MoveToAttribute(NODEKINDATTR, "");
                curKind = (NodeKind)System.Enum.Parse(typeof(NodeKind), nodeNav.Value);
                if (curKind == NodeKind.ValueNode)
                {
                    nodeNav.MoveToParent();
                    if (nodeNav.MoveToAttribute(VALUEREFATTR, ""))
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
            XPathNodeIterator edges = edgesRootNav.SelectChildren(EDGEELEM, "");
            List<EdgeRouteSpec> newNodeSpecs = new List<EdgeRouteSpec>();
            foreach (XPathNavigator edgeNav in edges)
            {
                newNodeSpecs.Add(new EdgeRouteSpec()
                {
                    FromNumber = Convert.ToInt32(edgeNav.Evaluate("number(@" + FROMATTR + ")")),
                    ToNumber = Convert.ToInt32(edgeNav.Evaluate("number(@" + TOATTR + ")")),
                    RequisiteFromNumber = Convert.ToInt32(edgeNav.Evaluate("number(@" + REQFROMATTR + ")")),
                    RequisiteToNumber = Convert.ToInt32(edgeNav.Evaluate("number(@" + REQTOATTR + ")"))
                });
            }
            return newNodeSpecs;
        }
    }
}
