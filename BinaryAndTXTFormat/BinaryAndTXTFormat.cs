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
        private const int NODEINDEXMAXIMUM = 3838;
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

        public BinaryAndTXTFormat(Stream binaryIn, TextReader TXTIn, NodeValueParser<NodeValType> nodeValueParser):
            this(nodeValueParser)
        {
            this.binaryIn = binaryIn;
            this.TXTIn = TXTIn;
        }

        public BinaryAndTXTFormat(NodeValueParser<NodeValType> nodeValueParser)
        {
            this.nodeValueParser = nodeValueParser;
        }

        public BinaryAndTXTFormat(string binaryFileName, string textFileName, NodeValueParser<NodeValType> nodeValueParser) :
            this(nodeValueParser)
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
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
            byte flags;
            NodeAndReqSpec lastSpec = null;
            NodeAndReqSpec currentNodeSpec = null;
            bool usePreviousEdgeAsReq = false;
            bool useStartEdgeAsReq = false;
            DataChunkRoute<NodeValType> writtenRoute = null;
            EdgeRoute reqEdge;
            int lastNewSequenceNumber = -1;
            for (int i = 0; i <= specs.Count - 1; i++)
            {
                currentNodeSpec = specs[i];
                if (currentNodeSpec.fromNode.SequenceNumber > NODEINDEXMAXIMUM)
                {
                    throw new Exception("Node index " + currentNodeSpec.fromNode.SequenceNumber +
                        " is over the supported maximum " + NODEINDEXMAXIMUM);
                }
                if (currentNodeSpec.fromNode.kind != NodeKind.ValueNode)
                {
                    flags = NOTVALUENODE;
                    writeSeqNumberAndFlags(writer, currentNodeSpec.fromNode.SequenceNumber, flags, lastNewSequenceNumber);
                    writtenRoute = sourceList.dataChunkRouteStartingAt((GateNode) sourceList.nodeByNumber(currentNodeSpec.fromNode.SequenceNumber));
                    lastNewSequenceNumber = currentNodeSpec.fromNode.SequenceNumber;
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
                        writeExistingNode(writer, currentNodeSpec.fromNode.SequenceNumber, 
                            currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq, lastNewSequenceNumber);
                    }
                    else
                    {
                        ValueNodeSpec<NodeValType> valSpec = currentNodeSpec.fromNode as ValueNodeSpec<NodeValType>;
                        IEnumerable<int> nodeIndexesWithValue =
                            destinationNodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                        if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.fromNode.SequenceNumber != nodeIndexesWithValue.First())
                        {
                            writeNewDuplicateValueNode(writer, currentNodeSpec.fromNode.SequenceNumber, 
                                currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq, nodeIndexesWithValue.First(),lastNewSequenceNumber);
                        }
                        else
                        {
                            writeNewUnduplicatedValueNode(writer, newValues, currentNodeSpec.fromNode.SequenceNumber, 
                                currentNodeSpec, usePreviousEdgeAsReq, useStartEdgeAsReq, valSpec, lastNewSequenceNumber);
                        }
                        lastNewSequenceNumber = currentNodeSpec.fromNode.SequenceNumber;
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

        private void writeExistingNode(BinaryWriter writer, int seqNumber, NodeAndReqSpec currentNodeSpec, 
            bool usePreviousEdgeAsReq, bool useStartEdgeAsReq, int lastNewSequenceNumber)
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
            writeSeqNumberAndFlags(writer, seqNumber, flags, lastNewSequenceNumber);
        }

        private void writeNewDuplicateValueNode(BinaryWriter writer, int seqNumber, NodeAndReqSpec currentNodeSpec, 
            bool usePreviousEdgeAsReq, bool useStartEdgeAsReq, int existingNodeWithValue, int lastNewSequenceNumber)
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
            writeSeqNumberAndFlags(writer, seqNumber, flags, lastNewSequenceNumber);
            writer.Write(indexToUshort(existingNodeWithValue));
        }

        private void writeNewUnduplicatedValueNode(BinaryWriter writer, List<NodeValType> newValues, int seqNumber, 
            NodeAndReqSpec currentNodeSpec, bool usePreviousEdgeAsReq, bool useStartEdgeAsReq, 
            ValueNodeSpec<NodeValType> valSpec, int lastNewSequenceNumber)
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
            writeSeqNumberAndFlags(writer, seqNumber, flags, lastNewSequenceNumber);
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

        private void writeSeqNumberAndFlags(BinaryWriter writer, int sequenceNumber, byte flags, int lastNewSequenceNumber)
        {
            ushort seqNumber = indexToUshort(sequenceNumber);
            var nextBytes = ushortAndHalfByteToSwappedBytes(seqNumber, flags);
            if (lastNewSequenceNumber != -1 &&
                sequenceNumber == lastNewSequenceNumber + 1)
            {
                // this is a signal to skip reading the second
                //   sequence number byte and instead increment
                //   the # for the last added node and use that
                writer.Write(switchOnLowerHalf(nextBytes.Item1));
            }
            else
            {
                writer.Write(nextBytes.Item1);
                writer.Write(nextBytes.Item2);
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
            int lastAddedSequenceNumber = -1;
            byte seqUpperHalfByte;
            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                var seqAndFlags = splitOutHalfBytes(r.ReadByte());
                flags = seqAndFlags.Item2;
                seqUpperHalfByte = seqAndFlags.Item1;
                // 15 is a signal to skip reading the next byte of the sequence number
                //   and instead just use the # of the last added node + 1
                if (seqUpperHalfByte == 15)
                {
                    seqNumInt = lastAddedSequenceNumber + 1;
                }
                else
                {
                    seqNum = swappedUshortToPlainUshort(seqUpperHalfByte, r.ReadByte());
                    seqNumInt = UshortToIndex(seqNum);
                }
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
                if (flags == NOTVALUENODE)
                {
                    lastAddedSequenceNumber = seqNumInt;
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
                        lastAddedSequenceNumber = seqNumInt;
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

        private Tuple<byte, byte> splitOutHalfBytes(byte orig)
        {
            byte upperHalf = (byte)(orig >> 4);
            byte lowerHalf = (byte)(orig & 15);
            return Tuple.Create(lowerHalf, upperHalf);
        }

        private Tuple<byte,byte> ushortAndHalfByteToSwappedBytes(ushort orig, byte halfByte)
        {
            byte[] origBytes = BitConverter.GetBytes(orig);
            origBytes[1] = (byte)(origBytes[1] | (halfByte << 4));
            return Tuple.Create(origBytes[1], origBytes[0]);
        }

        private ushort swappedUshortToPlainUshort(byte orig, byte orig2)
        {
            return BitConverter.ToUInt16(new byte[] { orig2, orig }, 0);
        }

        private byte switchOnLowerHalf(byte orig)
        {
            return (byte)(orig | 15);
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
