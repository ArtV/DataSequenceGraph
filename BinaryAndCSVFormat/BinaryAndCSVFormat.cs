using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using FileHelpers;
using System.IO;

namespace DataSequenceGraph.Format
{
    public class BinaryAndCSVFormat<NodeValType>
    {
        public NodeValueExporter<NodeValType> nodeValueExporter { get; set; }
        public NodeValueParser<NodeValType> nodeValueParser { get; set; }    

        public string binaryFileName { get; set; }
        public string textFileName { get; set; }

        public BinaryAndCSVFormat(string binaryFileName,string textFileName): 
            this(binaryFileName,textFileName,new ToStringNodeValueExporter<NodeValType>())
        {
        }

        public BinaryAndCSVFormat(string binaryFileName, string textFileName, NodeValueExporter<NodeValType> nodeValueExporter)
        {
            this.binaryFileName = binaryFileName;
            this.textFileName = textFileName;
            this.nodeValueExporter = nodeValueExporter;
        }

        public void ToBinaryAndCSV(MasterNodeList<NodeValType> nodeList)
        {
            ToBinaryAndCSV(nodeList, nodeList.AllNodeSpecs, nodeList.AllEdgeSpecs);
        }

        public void ToBinaryAndCSV(MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> edges)
        {
            IEnumerable<ValueNodeSpec<NodeValType>> valueNodeSpecs = nodes.OfType<ValueNodeSpec<NodeValType>>();
            var splittedSpecs = splitSpecs(nodeList, nodes);
            using (FileStream fileStream = new FileStream(binaryFileName, FileMode.Create))
            {
                using (BinaryWriter w = new BinaryWriter(fileStream))
                {
                    ToBinary(nodeList, splittedSpecs.Item1, edges, w);
                }                
            }
            ToCSV(splittedSpecs.Item2);
        }

        private Tuple<IList<Tuple<NodeSpec,int>>, IList<ValueNodeSpec<NodeValType>>> splitSpecs(
            MasterNodeList<NodeValType> nodeList, IEnumerable<NodeSpec> nodeSpecs)
        {
            IList<Tuple<NodeSpec,int>> binaryNodes = new List<Tuple<NodeSpec,int>>();
            IList<ValueNodeSpec<NodeValType>> textNodes = new List<ValueNodeSpec<NodeValType>>();
            foreach (NodeSpec currentNodeSpec in nodeSpecs)
            {
                if (currentNodeSpec.kind != NodeKind.ValueNode)
                {
                    binaryNodes.Add(Tuple.Create(currentNodeSpec, -1));
                }
                else
                {
                    ValueNodeSpec<NodeValType> valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    IEnumerable<int> nodeIndexesWithValue =
                        nodeList.getValueNodesByValue(valSpec.Value).Select(node => node.SequenceNumber);
                    if (nodeIndexesWithValue.Count() > 0 && currentNodeSpec.SequenceNumber != nodeIndexesWithValue.First())
                    {
                        binaryNodes.Add(Tuple.Create(currentNodeSpec,nodeIndexesWithValue.First()));
                    }
                    else
                    {
                        nodeIndexesWithValue = nodeSpecs.OfType<ValueNodeSpec<NodeValType>>().Where(
                            spec => spec.SequenceNumber < valSpec.SequenceNumber &&
                                    spec.Value.Equals(valSpec.Value)).Select(spec => spec.SequenceNumber);
                        if (nodeIndexesWithValue.Count() > 0 && valSpec.SequenceNumber != nodeIndexesWithValue.First())
                        {
                            binaryNodes.Add(Tuple.Create(currentNodeSpec, nodeIndexesWithValue.First()));
                        }
                        else
                        {
                            textNodes.Add(valSpec);
                        }
                    }
                }
            }
            return Tuple.Create(binaryNodes, textNodes);
        }

        private void ToBinary(MasterNodeList<NodeValType> nodeList, IEnumerable<Tuple<NodeSpec,int>> nodeSpecsAndValRefs, 
            IEnumerable<EdgeRouteSpec> edgeSpecs, BinaryWriter writer)
        {
            NodeSpec currentNodeSpec;
            ValueNodeSpec<NodeValType> valSpec;
            foreach(var specAndRef in nodeSpecsAndValRefs)
            {
                currentNodeSpec = specAndRef.Item1;
                writer.Write(indexToUInt16(currentNodeSpec.SequenceNumber));
                if (currentNodeSpec.kind != NodeKind.ValueNode)
                {
                    writer.Write(true);
                }
                else
                {
                    writer.Write(false);
                    valSpec = currentNodeSpec as ValueNodeSpec<NodeValType>;
                    writer.Write(indexToUInt16(specAndRef.Item2));
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
        }

        private UInt16 indexToUInt16(int ind)
        {
            return Convert.ToUInt16(ind + 1);
        }

        private int UInt16ToIndex(UInt16 shorty)
        {
            return Convert.ToInt32(shorty - 1);
        }

        private void ToCSV(IList<ValueNodeSpec<NodeValType>> nodeSpecs)
        {
            List<NodeValueRecord> arr = new List<NodeValueRecord>();

            NodeValueRecord record;

            foreach (ValueNodeSpec<NodeValType> valSpec in nodeSpecs)
            {
                record = new NodeValueRecord()
                {
                    SeqNum = valSpec.SequenceNumber,
                    NodeVal = nodeValueExporter.ToNodeValueString(valSpec.Value)
                };
                arr.Add(record);
            }          

            FileHelperEngine<NodeValueRecord> engine = new FileHelperEngine<NodeValueRecord>();

            engine.WriteFile(textFileName, arr.ToArray());
        }

        public MasterNodeList<NodeValType> ToNodeList()
        {
            return ToNodeList(new MasterNodeList<NodeValType>());
        }

        public MasterNodeList<NodeValType> ToNodeList(MasterNodeList<NodeValType> srcList)
        {
            IEnumerable<NodeSpec> binNodes = null;
            IEnumerable<EdgeRouteSpec> edges = null;
            srcList.reloadNodesFromSpecs(FromCSV());
            using (FileStream fs = new FileStream(binaryFileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader r = new BinaryReader(fs))
                {
                    binNodes = NodesFromBinary(srcList, r);
                    edges = EdgesFromBinary(r);
                }
            }
            srcList.reloadNodesThenRoutesFromSpecs(binNodes, edges);
            return srcList;
        }

        private IList<NodeSpec> NodesFromBinary(MasterNodeList<NodeValType> refList, BinaryReader r)
        {
            List<NodeSpec> newSpecs = new List<NodeSpec>();
            int valRefIndex;
            bool isGateNode;
            NodeSpec newSpec;
            ValueNodeSpec<NodeValType> newValSpec;
            UInt16 seqNum = r.ReadUInt16();
            while (seqNum != 0)
            {
                isGateNode = r.ReadBoolean();
                if (isGateNode)
                {
                    newSpec = new NodeSpec() { 
                        kind = NodeKind.GateNode, SequenceNumber = UInt16ToIndex(seqNum) 
                    };
                    newSpecs.Add(newSpec);
                }
                else
                {
                    valRefIndex = UInt16ToIndex(r.ReadUInt16());
                    newValSpec = new ValueNodeSpec<NodeValType>()
                    {
                        kind = NodeKind.ValueNode,
                        SequenceNumber = UInt16ToIndex(seqNum),
                        Value = ((ValueNode<NodeValType>)refList.nodeByNumber(valRefIndex)).Value
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

        private IList<ValueNodeSpec<NodeValType>> FromCSV()
        {
            if (nodeValueParser == null)
            {
                throw new InvalidOperationException("nodeValueParser must be set to recreate a graph");
            }
            FileHelperEngine<NodeValueRecord> engine = new FileHelperEngine<NodeValueRecord>();

            engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;

            NodeValueRecord[] res = engine.ReadFile(textFileName);

            if (engine.ErrorManager.ErrorCount > 0)
                engine.ErrorManager.SaveErrors("Errors.txt");

            List<ValueNodeSpec<NodeValType>> valSpecs = new List<ValueNodeSpec<NodeValType>>();
            foreach (NodeValueRecord rec in res)
            {
                valSpecs.Add(new ValueNodeSpec<NodeValType>()
                {
                    kind = NodeKind.ValueNode, SequenceNumber = rec.SeqNum,
                    Value = nodeValueParser.parseToValue(rec.NodeVal)
                });
            }

            return valSpecs;
        }
    }
}
