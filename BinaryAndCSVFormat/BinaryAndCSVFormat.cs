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
        private const byte DUPLICATEVALUENODE = 1;
        private const byte NEWVALUENODE = 2;
        private const byte EXISTINGVALUENODE = 3;

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
//            ToBinaryAndCSVFiles(new MasterNodeList<NodeValType>(), nodeList.AllNodeSpecs, nodeList.AllEdgeSpecs);
            ToBinaryAndCSVFiles(new MasterNodeList<NodeValType>(), nodeList.AllNodeAndReqSpecs);
        }
/*
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
*/
        public void ToBinaryAndCSVFiles(MasterNodeList<NodeValType> nodeList, IList<NodeAndReqSpec> specs)
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
                    ToBinaryAndCSVStreams(nodeList, specs);
                }
            }
        }

        public void ToBinaryAndCSVStreams(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndCSVStreams(new MasterNodeList<NodeValType>(), nodeList.AllNodeAndReqSpecs);
        }
/*
        public void ToBinaryAndCSVStreams(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> edges)
        {
            IEnumerable<ValueNodeSpec<NodeValType>> valueNodeSpecs = nodes.OfType<ValueNodeSpec<NodeValType>>();
            using (BinaryWriter w = new BinaryWriter(binaryOut))
            {
                ToCSV(ToBinary(nodeList, nodes, edges, w));
            }
        }
*/
        public void ToBinaryAndCSVStreams(MasterNodeList<NodeValType> nodeList, IList<NodeAndReqSpec> specs)
        {
            using (BinaryWriter w = new BinaryWriter(binaryOut))
            {
                ToCSV(ToBinary(nodeList, specs, w));
            }
        }

        private IList<NodeValType> ToBinary(MasterNodeList<NodeValType> nodeList, IList<NodeAndReqSpec> specs,
            BinaryWriter writer)
        {
            List<NodeValType> newValues = new List<NodeValType>();
            ushort seqNumber;
            byte flags;
            bool combineValueIndexAndReqIndex;
            ushort valueIndex;
            foreach (NodeAndReqSpec currentNodeSpec in specs)
            {
                valueIndex = 0;
                combineValueIndexAndReqIndex = false;
                seqNumber = indexToUshort(currentNodeSpec.fromNode.SequenceNumber);
                if (currentNodeSpec.fromNode.kind != NodeKind.ValueNode)
                {
                    flags = NOTVALUENODE;
                    writer.Write(ANDHalfByte(seqNumber,flags));
                }
                else
                {
                    if (!currentNodeSpec.insertFrom)
                    {
                        flags = EXISTINGVALUENODE;
                        writer.Write(ANDHalfByte(seqNumber, flags));
                    }
                    else
                    {
                        combineValueIndexAndReqIndex = true;
                        ValueNodeSpec<NodeValType> valSpec = currentNodeSpec.fromNode as ValueNodeSpec<NodeValType>;
                        IEnumerable<int> nodeIndexesWithValue =
                            nodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                        if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.fromNode.SequenceNumber != nodeIndexesWithValue.First())
                        {
                            flags = DUPLICATEVALUENODE;
                            writer.Write(ANDHalfByte(seqNumber, flags));
                            valueIndex = indexToUshort(nodeIndexesWithValue.First());
//                            writer.Write(indexToUshort(nodeIndexesWithValue.First()));
                        }
                        else
                        {
                            flags = NEWVALUENODE;
                            writer.Write(ANDHalfByte(seqNumber, flags));
                            int prevNewValueEntry = newValues.FindIndex(newVal => newVal.Equals(valSpec.Value));
                            if (prevNewValueEntry != -1)
                            {
                                valueIndex = indexToUshort(prevNewValueEntry);
//                                writer.Write(indexToUshort(prevNewValueEntry));
                            }
                            else
                            {
                                valueIndex = indexToUshort(newValues.Count);
//                                writer.Write(indexToUshort(newValues.Count));
                                newValues.Add(valSpec.Value);
                            }
                        }
                    }
                }

                if (combineValueIndexAndReqIndex)
                {
                    writeTwoUshortsToThreeBytes(valueIndex, indexToUshort(currentNodeSpec.ReqSequenceNumber),writer);
                }
                else
                {
                    writer.Write(indexToUshort(currentNodeSpec.ReqSequenceNumber));
                }
            }
            return newValues;
        }
/*
        private IList<NodeValType> ToBinary(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodeSpecs, 
            IEnumerable<EdgeRouteSpec> edgeSpecs, BinaryWriter writer)
        {
            List<NodeValType> newValues = new List<NodeValType>();
            ushort seqNumber;
            byte flags;
            foreach (NodeSpec currentNodeSpec in nodeSpecs)
            {
                seqNumber = indexToUshort(currentNodeSpec.SequenceNumber);
                if (currentNodeSpec.kind != NodeKind.ValueNode)
                {
                    flags = NOTVALUENODE;
                    writer.Write(ANDHalfByte(seqNumber, flags));
                }
                else
                {
                    ValueNodeSpec<NodeValType> valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    IEnumerable<int> nodeIndexesWithValue =
                        nodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                    if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.SequenceNumber != nodeIndexesWithValue.First())
                    {
                        flags = DUPLICATEVALUENODE;
                        writer.Write(ANDHalfByte(seqNumber, flags));
                        writer.Write(indexToUshort(nodeIndexesWithValue.First()));
                    }
                    else
                    {
                        flags = NEWVALUENODE;
                        writer.Write(ANDHalfByte(seqNumber, flags));
                        int prevNewValueEntry = newValues.FindIndex(newVal => newVal.Equals(valSpec.Value));
                        if (prevNewValueEntry != -1)
                        {
                            writer.Write(indexToUshort(prevNewValueEntry));
                        }
                        else
                        {
                            writer.Write(indexToUshort(newValues.Count));
                            newValues.Add(valSpec.Value);
                        }
                    }
                }
            }

            ushort separator = 0;
            writer.Write(separator);

            foreach (EdgeRouteSpec currentEdgeSpec in edgeSpecs)
            {
                writeTwoUshortsToThreeBytes(indexToUshort(currentEdgeSpec.FromNumber),
                    indexToUshort(currentEdgeSpec.ToNumber),writer);
                writeTwoUshortsToThreeBytes(indexToUshort(currentEdgeSpec.RequisiteFromNumber),
                    indexToUshort(currentEdgeSpec.RequisiteToNumber),writer);
            }

            return newValues;
        }
*/
        private ushort indexToUshort(int ind)
        {
            return Convert.ToUInt16(ind + 1);
        }

        private int UshortToIndex(ushort shorty)
        {
            return Convert.ToInt32(shorty - 1);
        }

        private void writeTwoUshortsToThreeBytes(ushort first,ushort second,BinaryWriter writer)
        {
            byte[] outBytes = twoUshortToThreeBytes(first, second);
            foreach (byte byt in outBytes)
            {
                writer.Write(byt);
            }
        }

        private byte[] twoUshortToThreeBytes(ushort first, ushort second)
        {
            byte[] firstBytes = BitConverter.GetBytes(first);
            byte[] secondBytes = BitConverter.GetBytes(second);

            byte byteThree = (byte)(firstBytes[1] + (secondBytes[1] << 4));

            return new byte[] { firstBytes[0], secondBytes[0], byteThree };
        }

        private ushort[] threeBytesToTwoUshort(byte first, byte second, byte third)
        {
            byte[] firstBytes = new byte[2];
            firstBytes[0] = first;
            byte[] secondBytes = new byte[2];
            secondBytes[0] = second;
            secondBytes[1] = (byte)(third >> 4);
            firstBytes[1] = (byte)(third & 15);
            return new ushort[] { BitConverter.ToUInt16(firstBytes, 0), BitConverter.ToUInt16(secondBytes, 0) };
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
//            IEnumerable<NodeSpec> binNodes = null;
//            IEnumerable<EdgeRouteSpec> edges = null;
            IList<NodeAndReqSpec> specs = null;
            IList<NodeValType> nodeValues = FromCSV();

            using (BinaryReader r = new BinaryReader(binaryIn))
            {
                specs = NodesFromBinary(srcList, nodeValues, r);
//                binNodes = NodesFromBinary(srcList, nodeValues, r);
//                edges = EdgesFromBinary(r);
            }

//            srcList.reloadNodesThenRoutesFromSpecs(binNodes, edges);
            srcList.reloadNodeAndReqSpecs(specs);
            return srcList;
        }

        private IList<NodeAndReqSpec> NodesFromBinary(MasterNodeList<NodeValType> refList, IList<NodeValType> nodeValues, BinaryReader r)
        {
            List<NodeAndReqSpec> newSpecs = new List<NodeAndReqSpec>();
            int valRefIndex;
            byte flags;
            NodeSpec newNonValueNodeSpec;
            NodeValType loadedVal;
            ValueNodeSpec<NodeValType> newValSpec;
            NodeAndReqSpec totalSpec;
            ushort seqNum;
            int reqIndex;
            ushort[] twoUShorts;
            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                var seqAndFlags = ReadAndSplitSeqAndFlags(r);
                seqNum = seqAndFlags.Item1;
                totalSpec = new NodeAndReqSpec()
                {
                    insertFrom = true,
                    ReqSequenceNumber = -1
                };
                flags = seqAndFlags.Item2;
                if (flags == NOTVALUENODE)
                {
                    newNonValueNodeSpec = new NodeSpec() { 
                        kind = NodeKind.GateNode, SequenceNumber = UshortToIndex(seqNum) 
                    };
                    totalSpec.fromNode = newNonValueNodeSpec;
                    reqIndex = UshortToIndex(r.ReadUInt16());
                }
                else
                {
                    if (flags == EXISTINGVALUENODE)
                    {
                        loadedVal = ((ValueNode<NodeValType>)refList.nodeByNumber(UshortToIndex(seqNum))).Value;
                        reqIndex = UshortToIndex(r.ReadUInt16());
                    }
                    else
                    {
                        twoUShorts = readTwoUshortsInThreeBytes(r);
                        valRefIndex = UshortToIndex(twoUShorts[0]);
                        reqIndex = UshortToIndex(twoUShorts[1]);
                        if (flags == DUPLICATEVALUENODE)
                        {
                            loadedVal = ((ValueNode<NodeValType>)refList.nodeByNumber(valRefIndex)).Value;
                        }
                        else  // == NEWVALUENODE
                        {
                            loadedVal = nodeValues[valRefIndex];
                        }
                    }
                    newValSpec = new ValueNodeSpec<NodeValType>()
                    {
                        kind = NodeKind.ValueNode,
                        SequenceNumber = UshortToIndex(seqNum),
                        Value = loadedVal
                    };
                    totalSpec.fromNode = newValSpec;
                }
                newSpecs.Add(totalSpec);


            }
            return newSpecs.AsReadOnly();
        }

        private Tuple<ushort, byte> ReadAndSplitSeqAndFlags(BinaryReader r)
        {
            return SplitOutHalfByte(r.ReadUInt16());
        }
/*
        private IEnumerable<EdgeRouteSpec> EdgesFromBinary(BinaryReader r)
        {
            List<EdgeRouteSpec> newEdges = new List<EdgeRouteSpec>();
            EdgeRouteSpec newNonValueNodeSpec;
            ushort[] twoUshorts;
            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                newNonValueNodeSpec = new EdgeRouteSpec();
                twoUshorts = readTwoUshortsInThreeBytes(r);
                newNonValueNodeSpec.FromNumber = UshortToIndex(twoUshorts[0]);
                newNonValueNodeSpec.ToNumber = UshortToIndex(twoUshorts[1]);

                twoUshorts = readTwoUshortsInThreeBytes(r);
                newNonValueNodeSpec.RequisiteFromNumber = UshortToIndex(twoUshorts[0]);
                newNonValueNodeSpec.RequisiteToNumber = UshortToIndex(twoUshorts[1]);
                newEdges.Add(newNonValueNodeSpec);
            }
            return newEdges;
        }
*/
        private ushort[] readTwoUshortsInThreeBytes(BinaryReader r)
        {
            byte[] threeStoredBytes = new byte[3];
            for (int i = 0; i <= 2; i++)
            {
                threeStoredBytes[i] = r.ReadByte();
            }

            return threeBytesToTwoUshort(threeStoredBytes[0], threeStoredBytes[1], threeStoredBytes[2]);
        }

        private ushort ANDHalfByte(ushort orig, byte upper)
        {
            byte[] origBytes = BitConverter.GetBytes(orig);
            origBytes[1] = (byte)(origBytes[1] | (upper << 4));
            return BitConverter.ToUInt16(origBytes, 0);
        }

        private Tuple<ushort, byte> SplitOutHalfByte(ushort orig)
        {
            byte[] origBytes = BitConverter.GetBytes(orig);
            byte upper = (byte)(origBytes[1] >> 4);
            origBytes[1] = (byte)(origBytes[1] & 15);
            return Tuple.Create(BitConverter.ToUInt16(origBytes, 0), upper);
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
