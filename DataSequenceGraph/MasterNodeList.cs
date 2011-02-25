using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node> nodeList = new List<Node>();
        private Dictionary<T,List<int>> valueSearchCache = new Dictionary<T,List<int>>();
        private List<GateNode> gateNodeList = new List<GateNode>();
        private RouteFactory<T> routeFactory;

        public Dictionary<Node, List<Route>> nodeRoutesDictionary { get; set; }

        public MasterNodeList()
        {
            routeFactory = new RouteFactory<T>();
            routeFactory.masterNodeList = this;
            nodeRoutesDictionary = new Dictionary<Node, List<Route>>();
        }

        public IEnumerable<Node> AllNodes
        {
            get
            {
                return nodeList;
            }
        }

        public IEnumerable<Node> AllNonNullNodes
        {
            get
            {
                return AllNodes.Where(node => node.kind != NodeKind.NullNode);
            }
        }

        public List<NodeSpec> AllNodeSpecs
        {
            get
            {
                return AllNonNullNodes.Select(node => node.ToNodeSpec()).ToList();
            }
        }

        public IEnumerable<EdgeRouteSpec> AllEdgeSpecs
        {
            get
            {
                return AllNonNullNodes.SelectMany(node => nodeToRoutesSpecs(node));
            }
        }

        private IEnumerable<EdgeRouteSpec> nodeToRoutesSpecs(Node node)
        {
            return node.OutgoingEdges.Select(route => route.ToEdgeRouteSpec());
        }

        public IList<NodeAndReqSpec> AllNodeAndReqSpecs
        {
            get
            {
                IList<NodeAndReqSpec> specs;
                MasterNodeList<T> newList = new MasterNodeList<T>();
                List<NodeAndReqSpec> overallList = new List<NodeAndReqSpec>();
                foreach (DataChunkRoute<T> route in enumerateDataChunkRoutes())
                {
                    specs = route.comboSpecsForMissingComponents(newList);
                    overallList.AddRange(specs);
                    newList.reloadNodeAndReqSpecs(specs);
                }
                return overallList.AsReadOnly();
            }
        }

        public IEnumerable<ValueNode<T>> getValueNodesByValue(T desiredValue)
        {
            if (!valueSearchCache.ContainsKey(desiredValue))
            {
                yield break;
            }
            else
            {
                foreach (int valueIndex in valueSearchCache[desiredValue])
                {
                    yield return (ValueNode<T>)nodeList[valueIndex];
                }
            }
        }

        public ValueNode<T> newValueNodeFromValue(T newValue)
        {
            ValueNode<T> newNode = new ValueNode<T>(newValue, nodeList.Count);
            AddNode(newNode);
            return newNode;
        }

        private void AddNode(Node newNode)
        {
            nodeList.Add(newNode);
            if (newNode is ValueNode<T>)
            {
                ValueNode<T> newValueNode = newNode as ValueNode<T>;
                cacheValueNode(newValueNode);
            }
            if (newNode is GateNode)
            {
                gateNodeList.Add(newNode as GateNode);
            }
        }

        private void cacheValueNode(ValueNode<T> newValueNode)
        {
            List<int> valueIndexList;
            if (!valueSearchCache.ContainsKey(newValueNode.Value))
            {
                valueIndexList = new List<int>();
                valueSearchCache.Add(newValueNode.Value, valueIndexList);
            }
            else
            {
                valueIndexList = valueSearchCache[newValueNode.Value];
            }
            valueIndexList.Add(newValueNode.SequenceNumber);
        }

        public Node nodeByNumber(int index)
        {
            if (index < 0 || index > nodeList.Count - 1)
            {
                return NullNode.o;
            }
            else
            {
                return nodeList[index];
            }
        }

        public GateNode newGateNode()
        {
            GateNode newNode = new GateNode(nodeList.Count);
            AddNode(newNode);
            return newNode;
        }

        public IEnumerable<IEnumerable<T>> enumerateDataChunks()
        {
            foreach (DataChunkRoute<T> route in enumerateDataChunkRoutes())
            {
                yield return route.dataChunk;
            }
        }

        public IEnumerable<DataChunkRoute<T>> enumerateDataChunkRoutes()
        {
            for (int i = 0; i <= gateNodeList.Count - 1; i++)
            {
                yield return nthDataChunkRoute(i);
            }
        }

        public DataChunkRoute<T> nthDataChunkRoute(int nth)
        {
            DataChunkRoute<T> chunkRoute = routeFactory.newDataChunkRoute(gateNodeList[nth]);
            chunkRoute.followToEnd();
            return chunkRoute;
        }

        public void reloadNodesFromSpecs(IEnumerable<NodeSpec> specs)
        {
            foreach (NodeSpec spec in specs)
            {
                trySetNode(spec);
            }
        }

        private Node nodeSpecToNode(NodeSpec spec)
        {
            Node returnNode;
            switch (spec.kind)
            {
                case NodeKind.GateNode:
                    returnNode = new GateNode(spec.SequenceNumber);
                    break;
                case NodeKind.ValueNode:
                    returnNode = new ValueNode<T>(((ValueNodeSpec<T>)spec).Value,spec.SequenceNumber);
                    break;
                default:
                    returnNode = NullNode.o;
                    break;
            }
            return returnNode;
        }

        public void reloadNodesThenRoutesFromSpecs(IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> routes)
        {
            reloadNodesFromSpecs(nodes);
            routeFactory.newRoutesFromSpecs(routes);
        }

        public void reloadNodeAndReqSpecs(IList<NodeAndReqSpec> specs)
        {
            EdgeRouteSpec edgeSpec;
            NodeAndReqSpec curSpec;
            NodeAndReqSpec nextSpec;
            int lastGateNodeIndex = -1;
            for (int i = 0; i <= specs.Count - 1; i++)
            {                
                curSpec = specs[i];
                if (curSpec.fromNode.kind == NodeKind.GateNode)
                {
                    lastGateNodeIndex = curSpec.fromNode.SequenceNumber;
                }
                if (curSpec.insertFrom)
                {
                    trySetNode(curSpec.fromNode);
                }
                if ((i + 1) <= specs.Count - 1)
                {
                    nextSpec = specs[i + 1];
                    if (nextSpec.insertFrom)
                    {
                        trySetNode(nextSpec.fromNode);
                    }
                    if (nextSpec.fromNode.kind == NodeKind.GateNode)
                    {
                        edgeSpec = new EdgeRouteSpec()
                        {
                            FromNumber = curSpec.fromNode.SequenceNumber,
                            ToNumber = lastGateNodeIndex,
                            RequisiteNumbers = new int[] { lastGateNodeIndex }
                        };
                        routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
                    }
                    else if (curSpec.ReqSequenceNumber != -1 || curSpec.fromNode.kind == NodeKind.GateNode)
                    {
                        edgeSpec = new EdgeRouteSpec()
                        {
                            FromNumber = curSpec.fromNode.SequenceNumber,
                            ToNumber = nextSpec.fromNode.SequenceNumber,
                            RequisiteNumbers = new int[] { curSpec.ReqSequenceNumber }
                        };
                        routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
                    }
                }                
            }
            edgeSpec = new EdgeRouteSpec()
            {
                FromNumber = specs[specs.Count - 1].fromNode.SequenceNumber,
                ToNumber = lastGateNodeIndex,
                RequisiteNumbers = new int[] { lastGateNodeIndex }
            };
            routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
        }

        public bool trySetNode(NodeSpec spec)
        {
            int desiredIndex = spec.SequenceNumber;
            if (desiredIndex <= nodeList.Count - 1)
            {
                if (desiredIndex < 0 || nodeList[desiredIndex].kind != NodeKind.NullNode ||
                    spec.kind == NodeKind.NullNode)
                {
                    return false;
                }
                else
                {
                    Node newNode = nodeSpecToNode(spec);
                    nodeList[desiredIndex] = newNode;
                    if (newNode is ValueNode<T>)
                    {
                        ValueNode<T> newValueNode = newNode as ValueNode<T>;
                        cacheValueNode(newValueNode);
                    }
                    if (newNode is GateNode)
                    {
                        gateNodeList.Add(newNode as GateNode);
                    }
                    return true;
                }
            }
            else
            {
                while (desiredIndex > nodeList.Count)
                {
                    nodeList.Add(NullNode.o);
                }
                AddNode(nodeSpecToNode(spec));
                return true;
            }
        }
    }
}
