using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using System.IO;

namespace DataSequenceGraph.Format
{
    public class BinaryAndCSVFormat<NodeValType>
    {
        private const short LINECHARLIMIT = 120;
        private const string DELIMITER = "|";

        private const byte NOTVALUENODE = 0;
        private const byte EXISTENTVALUENODE = 1;
        private const byte NEWVALUENODE = 2;

        public NodeValueExporter<NodeValType> nodeValueExporter { get; set; }
        public NodeValueParser<NodeValType> nodeValueParser { get; set; }    

        public string binaryFileName { get; set; }
        public string textFileName { get; set; }

        public TextWriter CSVOut { get; set; }
        public Stream binaryOut { get; set; }

        public TextReader CSVIn { get; set; }
        public Stream binaryIn { get; set; }

        public BinaryAndCSVFormat(NodeValueExporter<NodeValType> nodeValueExporter)
        {
            this.nodeValueExporter = nodeValueExporter;
        }

        public BinaryAndCSVFormat(string binaryFileName,string textFileName): 
            this(new ToStringNodeValueExporter<NodeValType>())
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
        }

        public BinaryAndCSVFormat(string binaryFileName, string textFileName, NodeValueExporter<NodeValType> nodeValueExporter):
            this(nodeValueExporter)
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
        }

        public BinaryAndCSVFormat(Stream binaryOut, TextWriter CSVOut) :
            this(new ToStringNodeValueExporter<NodeValType>())
        {
            this.binaryOut = binaryOut;
            this.CSVOut = CSVOut;
        }

        public BinaryAndCSVFormat(Stream binaryOut, TextWriter CSVOut, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            this.binaryOut = binaryOut;
            this.CSVOut = CSVOut;
        }

        public BinaryAndCSVFormat(Stream binaryIn, TextReader CSVIn, NodeValueParser<NodeValType> nodeValueParser)
        {
            this.binaryIn = binaryIn;
            this.CSVIn = CSVIn;
            this.nodeValueParser = nodeValueParser;
        }

        public void ToBinaryAndCSVFiles(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndCSVFiles(new MasterNodeList<NodeValType>(), nodeList.AllNodeSpecs, nodeList.AllEdgeSpecs);
        }

        public void ToBinaryAndCSVFiles(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> edges)
        {
            if (binaryFileName == null || binaryFileName.Equals("") ||
                textFileName == null || textFileName.Equals(""))
            {
                throw new InvalidOperationException("binary filename and CSV filename must be set first");
            }
            using (FileStream fileStream = new FileStream(binaryFileName, FileMode.Create))
            {
                using (TextWriter textWriter = new StreamWriter(new FileStream(textFileName, FileMode.Create)))
                {
                    binaryOut = fileStream;
                    CSVOut = textWriter;
                    ToBinaryAndCSVStreams(nodeList, nodes, edges);
                }
            }
        }

        public void ToBinaryAndCSVStreams(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndCSVStreams(new MasterNodeList<NodeValType>(), nodeList.AllNodeSpecs, nodeList.AllEdgeSpecs);
        }

        public void ToBinaryAndCSVStreams(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> edges)
        {
            IEnumerable<ValueNodeSpec<NodeValType>> valueNodeSpecs = nodes.OfType<ValueNodeSpec<NodeValType>>();
            using (BinaryWriter w = new BinaryWriter(binaryOut))
            {
                ToCSV(ToBinary(nodeList, nodes, edges, w));
            }
        }

        private IList<NodeValType> ToBinary(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodeSpecs, 
            IEnumerable<EdgeRouteSpec> edgeSpecs, BinaryWriter writer)
        {
            List<NodeValType> newValues = new List<NodeValType>();
            byte flags;
            foreach (NodeSpec currentNodeSpec in nodeSpecs)
            {
                writer.Write(indexToUInt16(currentNodeSpec.SequenceNumber));
                if (currentNodeSpec.kind != NodeKind.ValueNode)
                {
                    flags = NOTVALUENODE;
                    writer.Write(flags);
                }
                else
                {
                    ValueNodeSpec<NodeValType> valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    IEnumerable<int> nodeIndexesWithValue =
                        nodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                    if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.SequenceNumber != nodeIndexesWithValue.First())
                    {
                        flags = EXISTENTVALUENODE;
                        writer.Write(flags);
                        writer.Write(indexToUInt16(nodeIndexesWithValue.First()));
                    }
                    else
                    {
                        flags = NEWVALUENODE;
                        writer.Write(flags);
                        int prevNewValueEntry = newValues.FindIndex(newVal => newVal.Equals(valSpec.Value));
                        if (prevNewValueEntry != -1)
                        {
                            writer.Write(indexToUInt16(prevNewValueEntry));
                        }
                        else
                        {
                            writer.Write(indexToUInt16(newValues.Count));
                            newValues.Add(valSpec.Value);
                        }
                    }
                }
            }

            UInt16 separator = 0;
            writer.Write(separator);

            foreach (EdgeRouteSpec currentEdgeSpec in edgeSpecs)
            {
                writer.Write(indexToUInt16(currentEdgeSpec.FromNumber));
                writer.Write(indexToUInt16(currentEdgeSpec.ToNumber));
                writer.Write(indexToUInt16(currentEdgeSpec.RequisiteFromNumber));
                writer.Write(indexToUInt16(currentEdgeSpec.RequisiteToNumber));
            }

            return newValues;
        }

        private UInt16 indexToUInt16(int ind)
        {
            return Convert.ToUInt16(ind + 1);
        }

        private int UInt16ToIndex(UInt16 shorty)
        {
            return Convert.ToInt32(shorty - 1);
        }

        private void ToCSV(IList<NodeValType> nodeValues)
        {
            int lineCounter = 0;
            string nextVal;
            bool firstOne = true;
            foreach (NodeValType nodeValue in nodeValues)
            {
                nextVal = nodeValueExporter.ToNodeValueString(nodeValue);
                if (lineCounter + (nextVal.Length + 1) > LINECHARLIMIT)
                {
                    CSVOut.WriteLine();
                    CSVOut.Write(nextVal);
                    lineCounter = nextVal.Length;
                }
                else
                {
                    if (!firstOne)
                    {
                        CSVOut.Write(DELIMITER);
                    }
                    CSVOut.Write(nextVal);
                    lineCounter += (nextVal.Length + 1);
                }
                firstOne = false;
            }          
        }

        public MasterNodeList<NodeValType> ToNodeListFromFiles()
        {
            return ToNodeListFromFiles(new MasterNodeList<NodeValType>());
        }

        public MasterNodeList<NodeValType> ToNodeListFromStreams()
        {
            return ToNodeListFromStreams(new MasterNodeList<NodeValType>());
        }

        public MasterNodeList<NodeValType> ToNodeListFromFiles(MasterNodeList<NodeValType> srcList)
        {
            if (binaryFileName == null || binaryFileName.Equals("") ||
                textFileName == null || textFileName.Equals(""))
            {
                throw new InvalidOperationException("binary filename and CSV filename must be set first");
            }
            using (FileStream fs = new FileStream(binaryFileName, FileMode.Open, FileAccess.Read))
            {
                using (TextReader textReader = new StreamReader(new FileStream(textFileName, FileMode.Open, FileAccess.Read)))
                {
                    binaryIn = fs;
                    CSVIn = textReader;
                    return ToNodeListFromStreams(srcList);
                }
            }

        }

        public MasterNodeList<NodeValType> ToNodeListFromStreams(MasterNodeList<NodeValType> srcList)
        {
            IEnumerable<NodeSpec> binNodes = null;
            IEnumerable<EdgeRouteSpec> edges = null;
            IList<NodeValType> nodeValues = FromCSV();

            using (BinaryReader r = new BinaryReader(binaryIn))
            {
                binNodes = NodesFromBinary(srcList, nodeValues, r);
                edges = EdgesFromBinary(r);
            }

            srcList.reloadNodesThenRoutesFromSpecs(binNodes, edges);
            return srcList;
        }

        private IList<NodeSpec> NodesFromBinary(MasterNodeList<NodeValType> refList, IList<NodeValType> nodeValues, BinaryReader r)
        {
            List<NodeSpec> newSpecs = new List<NodeSpec>();
            int valRefIndex;
            byte flags;
            NodeSpec newSpec;
            NodeValType loadedVal;
            ValueNodeSpec<NodeValType> newValSpec;
            UInt16 seqNum = r.ReadUInt16();
            while (seqNum != 0)
            {
                flags = r.ReadByte();
                if (flags == NOTVALUENODE)
                {
                    newSpec = new NodeSpec() { 
                        kind = NodeKind.GateNode, SequenceNumber = UInt16ToIndex(seqNum) 
                    };
                    newSpecs.Add(newSpec);
                }
                else
                {
                    valRefIndex = UInt16ToIndex(r.ReadUInt16());
                    if (flags == EXISTENTVALUENODE)
                    {
                        loadedVal = ((ValueNode<NodeValType>)refList.nodeByNumber(valRefIndex)).Value;
                    }
                    else
                    {
                        loadedVal = nodeValues[valRefIndex];
                    }
                    newValSpec = new ValueNodeSpec<NodeValType>()
                    {
                        kind = NodeKind.ValueNode,
                        SequenceNumber = UInt16ToIndex(seqNum),
                        Value = loadedVal
                    };
                    newSpecs.Add(newValSpec);
                }
                seqNum = r.ReadUInt16();
            }
            return newSpecs;
        }

        private IEnumerable<EdgeRouteSpec> EdgesFromBinary(BinaryReader r)
        {
            List<EdgeRouteSpec> newEdges = new List<EdgeRouteSpec>();
            EdgeRouteSpec newSpec;
            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                newSpec = new EdgeRouteSpec();
                newSpec.FromNumber = UInt16ToIndex(r.ReadUInt16());
                newSpec.ToNumber = UInt16ToIndex(r.ReadUInt16());
                newSpec.RequisiteFromNumber = UInt16ToIndex(r.ReadUInt16());
                newSpec.RequisiteToNumber = UInt16ToIndex(r.ReadUInt16());
                newEdges.Add(newSpec);
            }
            return newEdges;
        }

        private IList<NodeValType> FromCSV()
        {
            if (nodeValueParser == null)
            {
                throw new InvalidOperationException("nodeValueParser must be set to recreate a graph");
            }
            List<NodeValType> nodeValues = new List<NodeValType>();
            string nextLine;
            char[] charD = DELIMITER.Substring(0,1).ToCharArray();
            while ((nextLine = CSVIn.ReadLine()) != null)
            {
                nodeValues.AddRange(nextLine.Split(charD).
                    Select(strV => nodeValueParser.parseToValue(strV)));
            }

            return nodeValues;
        }
    }
}
