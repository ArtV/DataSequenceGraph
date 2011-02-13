using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DataSequenceGraph;

namespace DataSequenceGraph.Format
{
    public class XMLGraphFormat<NodeValType>
    {
        public NodeValueConverter<NodeValType> nodeValueConverter { get; set; }

        public XMLGraphFormat()
        {
            this.nodeValueConverter = new ToStringNodeValueConverter<NodeValType>();
        }

        public XMLGraphFormat(NodeValueConverter<NodeValType> nodeValueConverter)
        {
            this.nodeValueConverter = nodeValueConverter;
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
            for (nodeIndex = 0; nodeIndex <= nodeSpecs.Count - 1; nodeIndex++)
            {
                currentNodeSpec = nodeSpecs[nodeIndex];
                nodeElement = doc.CreateNode(XmlNodeType.Element, "Node", "");
                rootNodesElement.AppendChild(nodeElement);

                IDAttr = doc.CreateAttribute("ID");
                IDAttr.Value = nodeIndex.ToString();
                nodeElement.Attributes.Append(IDAttr);

                kindAttr = doc.CreateAttribute("Kind");
                kindAttr.Value = currentNodeSpec.kind.ToString();
                nodeElement.Attributes.Append(kindAttr);

                if (currentNodeSpec.kind == NodeKind.ValueNode)
                {
                    valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    valueText = doc.CreateTextNode(nodeValueConverter.ToNodeValueString(valSpec.Value));
                    nodeElement.AppendChild(valueText);
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
    }
}
