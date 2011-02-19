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

        public List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>> AllNodeAndRouteSpecs
        {
            get
            {
                List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>> retList = new List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>>();
                foreach (Node node in AllNonNullNodes)
                {
                    Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>> retElem = 
                        new Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>(node.ToNodeSpec(), nodeToRoutesSpecs(node));                    
                    retList.Add(retElem);
                }
                return retList;
            }
        }        

        private IEnumerable<EdgeRouteSpec> nodeToRoutesSpecs(Node node)
        {
            return node.OutgoingRoutes.Select(route => route.ToEdgeRouteSpec());
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
            return nodeList[index];
        }

        public GateNode newGateNode()
        {
            GateNode newNode = new GateNode(nodeList.Count);
            AddNode(newNode);
            return newNode;
        }

        public IEnumerable<IEnumerable<T>> enumerateDataChunks()
        {
            DataChunkRoute<T> chunkRoute;
            RouteFactory<T> routeFactory = new RouteFactory<T>();
            foreach (GateNode node in nodeList.OfType<GateNode>())
            {
                chunkRoute = routeFactory.newDataChunkRoute(node);
                chunkRoute.followToEnd();
                yield return chunkRoute.dataChunk;
            }
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
            RouteFactory<T> factory = new RouteFactory<T>();
            factory.masterNodeList = this;
            factory.newRoutesFromSpecs(routes);
        }

        public bool trySetNode(NodeSpec spec)
        {
            int desiredIndex = spec.SequenceNumber;
            if (desiredIndex <= nodeList.Count - 1)
            {
                if (desiredIndex < 0 || nodeList[desiredIndex].kind != NodeKind.NullNode)
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
