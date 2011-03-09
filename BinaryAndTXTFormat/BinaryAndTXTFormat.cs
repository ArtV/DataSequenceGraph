using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using System.IO;

namespace DataSequenceGraph.Format
{
    public class BinaryAndTXTFormat<NodeValType>
    {
        private const short LINECHARLIMIT = 120;
        private const string DELIMITER = "|";

        private const byte NOTVALUENODE = 0;
        private const byte DUPLICATEVALUENODE = 1;
        private const byte NEWVALUENODE = 2;
        private const byte EXISTINGVALUENODE = 3;

        private const byte DUPLICATEVALUENODEPREVEDGEREQ = 4;
        private const byte NEWVALUENODEPREVEGDEREQ = 5;
        private const byte EXISTINGVALUENODEPREVEDGEREQ = 6;

        private const byte DUPLICATEVALUENODENOTEDGE = 7;
        private const byte NEWVALUENODENOTEDGE = 8;
        private const byte EXISTINGVALUENODENOTEDGE = 9;

        private const byte DUPLICATEVALUENODESTARTEDGE = 10;
        private const byte NEWVALUENODESTARTEDGE = 11;
        private const byte EXISTINGVALUENODESTARTEDGE = 12;

        public NodeValueExporter<NodeValType> nodeValueExporter { get; set; }
        public NodeValueParser<NodeValType> nodeValueParser { get; set; }    

        public string binaryFileName { get; set; }
        public string textFileName { get; set; }

        public TextWriter TXTOut { get; set; }
        public Stream binaryOut { get; set; }

        public TextReader TXTIn { get; set; }
        public Stream binaryIn { get; set; }

        public BinaryAndTXTFormat(NodeValueExporter<NodeValType> nodeValueExporter)
        {
            this.nodeValueExporter = nodeValueExporter;
        }

        public BinaryAndTXTFormat(string binaryFileName,string textFileName): 
            this(new ToStringNodeValueExporter<NodeValType>())
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
        }

        public BinaryAndTXTFormat(string binaryFileName, string textFileName, NodeValueExporter<NodeValType> nodeValueExporter):
            this(nodeValueExporter)
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
        }

        public BinaryAndTXTFormat(Stream binaryOut, TextWriter TXTOut) :
            this(new ToStringNodeValueExporter<NodeValType>())
        {
            this.binaryOut = binaryOut;
            this.TXTOut = TXTOut;
        }

        public BinaryAndTXTFormat(Stream binaryOut, TextWriter TXTOut, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            this.binaryOut = binaryOut;
            this.TXTOut = TXTOut;
        }

        public BinaryAndTXTFormat(Stream binaryIn, TextReader TXTIn, NodeValueParser<NodeValType> nodeValueParser)
        {
            this.binaryIn = binaryIn;
            this.TXTIn = TXTIn;
            this.nodeValueParser = nodeValueParser;
        }

        public void ToBinaryAndTXTFiles(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndTXTFiles(nodeList,new MasterNodeList<NodeValType>(), nodeList.AllNodeAndReqSpecs);
        }

        public void ToBinaryAndTXTFiles(MasterNodeList<NodeValType> sourceNodeList, MasterNodeList<NodeValType> destinationNodeList, IList<NodeAndReqSpec> specs)
        {
            if (binaryFileName == null || binaryFileName.Equals("") ||
                textFileName == null || textFileName.Equals(""))
            {
                throw new InvalidOperationException("binary filename and TXT filename must be set first");
            }
            using (FileStream fileStream = new FileStream(binaryFileName, FileMode.Create))
            {
                using (TextWriter textWriter = new StreamWriter(new FileStream(textFileName, FileMode.Create)))
                {
                    binaryOut = fileStream;
                    TXTOut = textWriter;
                    ToBinaryAndTXTStreams(sourceNodeList,destinationNodeList, specs);
                }
            }
        }

        public void ToBinaryAndTXTStreams(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndTXTStreams(nodeList,new MasterNodeList<NodeValType>(), nodeList.AllNodeAndReqSpecs);
        }

        public void ToBinaryAndTXTStreams(MasterNodeList<NodeValType> sourceList, MasterNodeList<NodeValType> destinationList, IList<NodeAndReqSpec> specs)
        {
            using (BinaryWriter w = new BinaryWriter(binaryOut))
            {
                ToTXT(ToBinary(sourceList,destinationList, specs, w));
            }
        }

        private IList<NodeValType> ToBinary(MasterNodeList<NodeValType> sourceList, MasterNodeList<NodeValType> destinationNodeList, IList<NodeAndReqSpec> specs,
            BinaryWriter writer)
        {
            List<NodeValType> newValues = new List<NodeValType>();
            ushort seqNumber;
            byte flags;
            NodeAndReqSpec lastSpec = null;
            NodeAndReqSpec currentNodeSpec = null;
            bool usePreviousEdgeAsReq = false;
            bool useStartEdgeAsReq = false;
            DataChunkRoute<NodeValType> writtenRoute = null;
            EdgeRoute reqEdge;
            for (int i = 0; i <= specs.Count - 1; i++)
            {
                currentNodeSpec = specs[i];
                seqNumber = indexToUshort(currentNodeSpec.fromNode.SequenceNumber);
                if (currentNodeSpec.fromNode.kind != NodeKind.ValueNode)
                {
                    flags = NOTVALUENODE;
                    writer.Write(ANDHalfByte(seqNumber,flags));
                    writtenRoute = sourceList.dataChunkRouteStartingAt((GateNode) sourceList.nodeByNumber(currentNodeSpec.fromNode.SequenceNumber));
                    // skip writing requisite node numbers since those must be -1, -1
                }
                else
                {
                    usePreviousEdgeAsReq = false;
                    useStartEdgeAsReq = false;
                    if (currentNodeSpec.reqFromRouteIndex != -1)
                    {
                        reqEdge = writtenRoute.componentEdges[currentNodeSpec.reqFromRouteIndex];
                        usePreviousEdgeAsReq = (lastSpec != null &&
                            reqEdge.edge.link.from.SequenceNumber == lastSpec.fromNode.SequenceNumber &&
                            reqEdge.edge.link.to.SequenceNumber == currentNodeSpec.fromNode.SequenceNumber);
                        useStartEdgeAsReq = (i > 0 &&
                            reqEdge.edge.link.from.SequenceNumber == specs[0].fromNode.SequenceNumber &&
                            reqEdge.edge.link.to.SequenceNumber == specs[1].fromNode.SequenceNumber);
                    }
                    if (!currentNodeSpec.insertFrom)
                    {
                        writeExistingNode(writer, seqNumber, currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq);
                    }
                    else
                    {
                        ValueNodeSpec<NodeValType> valSpec = currentNodeSpec.fromNode as ValueNodeSpec<NodeValType>;
                        IEnumerable<int> nodeIndexesWithValue =
                            destinationNodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                        if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.fromNode.SequenceNumber != nodeIndexesWithValue.First())
                        {
                            writeNewDuplicateValueNode(writer, seqNumber, currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq, nodeIndexesWithValue.First());
                        }
                        else
                        {
                            writeNewUnduplicatedValueNode(writer, newValues, seqNumber, currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq, valSpec);
                        }
                    }
                    if (!usePreviousEdgeAsReq && currentNodeSpec.reqFromRouteIndex != -1 &&
                        !useStartEdgeAsReq)
                    {
                        writer.Write((byte)currentNodeSpec.reqFromRouteIndex);
                    }
                }

                lastSpec = currentNodeSpec;
            }
            return newValues;
        }

        private void writeExistingNode(BinaryWriter writer, ushort seqNumber, NodeAndReqSpec currentNodeSpec, bool usePreviousEdgeAsReq, bool useStartEdgeAsReq)
        {
            byte flags;
            if (currentNodeSpec.reqFromRouteIndex == -1)
            {
                flags = EXISTINGVALUENODENOTEDGE;
            }
            else if (usePreviousEdgeAsReq)
            {
                flags = EXISTINGVALUENODEPREVEDGEREQ;
            }
            else if (useStartEdgeAsReq)
            {
                flags = EXISTINGVALUENODESTARTEDGE;
            }
            else
            {
                flags = EXISTINGVALUENODE;
            }
            writer.Write(ANDHalfByte(seqNumber, flags));
        }

        private void writeNewDuplicateValueNode(BinaryWriter writer, ushort seqNumber, NodeAndReqSpec currentNodeSpec, bool usePreviousEdgeAsReq, bool useStartEdgeAsReq, int existingNodeWithValue)
        {
            byte flags;
            if (currentNodeSpec.reqFromRouteIndex == -1)
            {
                flags = DUPLICATEVALUENODENOTEDGE;
            }
            else if (usePreviousEdgeAsReq)
            {
                flags = DUPLICATEVALUENODEPREVEDGEREQ;
            }
            else if (useStartEdgeAsReq)
            {
                flags = DUPLICATEVALUENODESTARTEDGE;
            }
            else
            {
                flags = DUPLICATEVALUENODE;
            }
            writer.Write(ANDHalfByte(seqNumber, flags));
            writer.Write(indexToUshort(existingNodeWithValue));
        }

        private void writeNewUnduplicatedValueNode(BinaryWriter writer, List<NodeValType> newValues, ushort seqNumber, NodeAndReqSpec currentNodeSpec, bool usePreviousEdgeAsReq, bool useStartEdgeAsReq, ValueNodeSpec<NodeValType> valSpec)
        {
            byte flags;
            if (currentNodeSpec.reqFromRouteIndex == -1)
            {
                flags = NEWVALUENODENOTEDGE;
            }
            else if (usePreviousEdgeAsReq)
            {
                flags = NEWVALUENODEPREVEGDEREQ;
            }
            else if (useStartEdgeAsReq)
            {
                flags = NEWVALUENODESTARTEDGE;
            }
            else
            {
                flags = NEWVALUENODE;
            }
            writer.Write(ANDHalfByte(seqNumber, flags));
            int prevNewValueEntry = newValues.FindIndex(newVal => newVal.Equals(valSpec.Value));
            if (prevNewValueEntry != -1)
            {
                writer.Write(indexToUshort(prevNewValueEntry));
            }
            else
            {
                int newIndex = newValues.Count;
                newValues.Add(valSpec.Value);
                writer.Write(indexToUshort(newIndex));
            }
        }

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

            byte byteThree = (byte)(firstBytes[1] | (secondBytes[1] << 4));

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

        private void ToTXT(IList<NodeValType> nodeValues)
        {
            int lineCounter = 0;
            string nextVal;
            bool firstOne = true;
            foreach (NodeValType nodeValue in nodeValues)
            {
                nextVal = nodeValueExporter.ToNodeValueString(nodeValue);
                if (lineCounter + (nextVal.Length + 1) > LINECHARLIMIT)
                {
                    TXTOut.WriteLine();
                    TXTOut.Write(nextVal);
                    lineCounter = nextVal.Length;
                }
                else
                {
                    if (!firstOne)
                    {
                        TXTOut.Write(DELIMITER);
                    }
                    TXTOut.Write(nextVal);
                    lineCounter += (nextVal.Length + 1);
                }
                firstOne = false;
            }          
        }

        public MasterNodeList<NodeValType> ToNodeListFromFiles()
        {
            MasterNodeList<NodeValType> retList = new MasterNodeList<NodeValType>();
            ToNodeListFromFiles(retList);
            return retList;
        }

        public MasterNodeList<NodeValType> ToNodeListFromStreams()
        {
            MasterNodeList<NodeValType> retList = new MasterNodeList<NodeValType>();
            ToNodeListFromStreams(retList);
            return retList;
        }

        public void ToNodeListFromFiles(MasterNodeList<NodeValType> srcList)
        {
            if (binaryFileName == null || binaryFileName.Equals("") ||
                textFileName == null || textFileName.Equals(""))
            {
                throw new InvalidOperationException("binary filename and TXT filename must be set first");
            }
            using (FileStream fs = new FileStream(binaryFileName, FileMode.Open, FileAccess.Read))
            {
                using (TextReader textReader = new StreamReader(new FileStream(textFileName, FileMode.Open, FileAccess.Read)))
                {
                    binaryIn = fs;
                    TXTIn = textReader;
                    ToNodeListFromStreams(srcList);
                }
            }

        }

        public void ToNodeListFromStreams(MasterNodeList<NodeValType> srcList)
        {
            IList<NodeValType> nodeValues = FromTXT();

            using (BinaryReader r = new BinaryReader(binaryIn))
            {
                NodesFromBinary(srcList, nodeValues, r);
            }
        }

        private void  NodesFromBinary(MasterNodeList<NodeValType> refList, IList<NodeValType> nodeValues, BinaryReader r)
        {
            List<NodeAndReqSpec> newSpecs = new List<NodeAndReqSpec>();
            byte flags;
            NodeSpec newNonValueNodeSpec;
            NodeValType loadedVal;
            ValueNodeSpec<NodeValType> newValSpec;
            NodeAndReqSpec totalSpec;
            ushort seqNum;
            int seqNumInt;
            int prevFromIndex = -1;
            int startNodeIndex = -1;
            int secondNodeIndex = -1;
            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                var seqAndFlags = ReadAndSplitSeqAndFlags(r);
                seqNum = seqAndFlags.Item1;
                seqNumInt = UshortToIndex(seqNum);
                if (startNodeIndex == -1)
                {
                    startNodeIndex = seqNumInt;
                }
                else if (secondNodeIndex == -1)
                {
                    secondNodeIndex = seqNumInt;
                }
                totalSpec = new NodeAndReqSpec()
                {
                    insertFrom = true,
                    reqFromRouteIndex = -1,
                    usePrevEdgeAsReq = false,
                    useStartEdgeAsReq = false,                    
                };
                flags = seqAndFlags.Item2;
                if (flags == NOTVALUENODE)
                {
                    if (newSpecs.Count > 1)
                    {
                        refList.reloadNodeAndReqSpecs(newSpecs);
                        newSpecs = new List<NodeAndReqSpec>();
                    }
                    newNonValueNodeSpec = new NodeSpec() { 
                        kind = NodeKind.GateNode, SequenceNumber = seqNumInt 
                    };
                    totalSpec.fromNode = newNonValueNodeSpec;
                }
                else
                {
                    if (flags == EXISTINGVALUENODE || flags == EXISTINGVALUENODEPREVEDGEREQ
                        || flags == EXISTINGVALUENODENOTEDGE || flags == EXISTINGVALUENODESTARTEDGE)
                    {
                        loadedVal = ((ValueNode<NodeValType>)refList.nodeByNumber(seqNumInt)).Value;
                        readExistingValueNode(refList, r, flags, seqNumInt, prevFromIndex, startNodeIndex, secondNodeIndex, totalSpec);
                    }
                    else
                    {
                        readNewValueNode(refList, r, flags, seqNumInt, prevFromIndex, startNodeIndex, secondNodeIndex, totalSpec);
                        loadedVal = loadValueFromIndex(refList, r, flags, nodeValues);
                    }
                    newValSpec = new ValueNodeSpec<NodeValType>()
                    {
                        kind = NodeKind.ValueNode,
                        SequenceNumber = seqNumInt,
                        Value = loadedVal
                    };
                    totalSpec.fromNode = newValSpec;
                }
                newSpecs.Add(totalSpec);
                prevFromIndex = totalSpec.fromNode.SequenceNumber;
            }
            refList.reloadNodeAndReqSpecs(newSpecs);
        }

        private void readExistingValueNode(MasterNodeList<NodeValType> refList, BinaryReader r, byte flags, int seqNumInt, int prevFromIndex, int startNodeIndex, int secondNodeIndex, NodeAndReqSpec totalSpec)
        {
            if (flags == EXISTINGVALUENODENOTEDGE)
            {
                totalSpec.reqFromRouteIndex = -1;
            }
            else if (flags == EXISTINGVALUENODEPREVEDGEREQ)
            {
                totalSpec.usePrevEdgeAsReq = true;
            }
            else if (flags == EXISTINGVALUENODESTARTEDGE)
            {
                totalSpec.useStartEdgeAsReq = true;
            }
            else // == EXISTINGVALUENODE
            {
                byte reqEdgeIndex = r.ReadByte();
                totalSpec.reqFromRouteIndex = reqEdgeIndex;
            }
        }

        private void readNewValueNode(MasterNodeList<NodeValType> refList, BinaryReader r, byte flags, int seqNumInt, int prevFromIndex, int startNodeIndex, int secondNodeIndex, NodeAndReqSpec totalSpec)
        {
            if (flags == NEWVALUENODENOTEDGE || flags == DUPLICATEVALUENODENOTEDGE)
            {
                totalSpec.reqFromRouteIndex = -1;
            }
            else if (flags == NEWVALUENODEPREVEGDEREQ || flags == DUPLICATEVALUENODEPREVEDGEREQ)
            {
                totalSpec.usePrevEdgeAsReq = true;
            }
            else if (flags == NEWVALUENODESTARTEDGE || flags == DUPLICATEVALUENODESTARTEDGE)
            {
                totalSpec.useStartEdgeAsReq = true;
            }
            else
            {
                byte reqEdgeIndex = r.ReadByte();
                totalSpec.reqFromRouteIndex = reqEdgeIndex;
            }
        }

        private NodeValType loadValueFromIndex(MasterNodeList<NodeValType> refList, BinaryReader r, byte flags, IList<NodeValType> nodeValues)
        {
            if (flags == DUPLICATEVALUENODE || flags == DUPLICATEVALUENODENOTEDGE ||
                flags == DUPLICATEVALUENODEPREVEDGEREQ || flags == DUPLICATEVALUENODESTARTEDGE)
            {
                return ((ValueNode<NodeValType>)refList.nodeByNumber(UshortToIndex(r.ReadUInt16()))).Value;
            }
            else  // == NEWVALUENODE, NEWVALUENODENOTEGDE, NEWVALUENODEPREVEDGEREQ, NEWVALUENODESTARTEDGE
            {
                return nodeValues[UshortToIndex(r.ReadUInt16())];
            }
        }

        private Tuple<ushort, byte> ReadAndSplitSeqAndFlags(BinaryReader r)
        {
            return SplitOutHalfByte(r.ReadUInt16());
        }

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

        private IList<NodeValType> FromTXT()
        {
            if (nodeValueParser == null)
            {
                throw new InvalidOperationException("nodeValueParser must be set to recreate a graph");
            }
            List<NodeValType> nodeValues = new List<NodeValType>();
            string nextLine;
            char[] charD = DELIMITER.Substring(0,1).ToCharArray();
            while ((nextLine = TXTIn.ReadLine()) != null)
            {
                nodeValues.AddRange(nextLine.Split(charD).
                    Select(strV => nodeValueParser.parseToValue(strV)));
            }

            return nodeValues;
        }
    }
}
