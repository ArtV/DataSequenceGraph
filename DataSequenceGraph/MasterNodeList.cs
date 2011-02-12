using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node> nodeList = new List<Node>();

        public IEnumerable<Node> AllNodes
        {
            get
            {
                return nodeList;
            }
        }

        public List<NodeSpec> AllNodeSpecs
        {
            get
            {
                return nodeList.Select(node => node.ToNodeSpec()).ToList();
            }
        }

        public IEnumerable<EdgeRouteSpec> AllEdgeSpecs
        {
            get
            {
                return nodeList.SelectMany(node => nodeToRoutesSpecs(node));
            }
        }

        public List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>> AllNodeAndRouteSpecs
        {
            get
            {
                List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>> retList = new List<Tuple<NodeSpec, IEnumerable<EdgeRouteSpec>>>();
                foreach (Node node in nodeList)
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
            return nodeList.OfType<ValueNode<T>>().Where(node => node.Value.Equals(desiredValue));
        }

        public ValueNode<T> newValueNodeFromValue(T newValue)
        {
            ValueNode<T> newNode = new ValueNode<T>(newValue, nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }

        public Node nodeByNumber(int index)
        {
            return nodeList.ElementAt<Node>(index);
        }

        public StartNode newStartNode()
        {
            StartNode newNode = new StartNode(nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }

        public EndNode newEndNode()
        {
            EndNode newNode = new EndNode(nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }
        
        public IEnumerable<IEnumerable<T>> enumerateDataChunks()
        {
            DataChunkRoute<T> chunkRoute;
            RouteFactory<T> routeFactory = new RouteFactory<T>();
            foreach (StartNode node in nodeList.OfType<StartNode>())
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
                switch (spec.kind)
                {
                    case NodeKind.StartNode:
                        newStartNode();
                        break;
                    case NodeKind.EndNode:
                        newEndNode();
                        break;
                    case NodeKind.NullNode:
                        break;
                    case NodeKind.ValueNode:
                        newValueNodeFromValue(((ValueNodeSpec<T>) spec).Value);
                        break;
                }
            }
        }

        public void reloadNodesThenRoutesFromSpecs(IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> routes)
        {
            reloadNodesFromSpecs(nodes);
            RouteFactory<T> factory = new RouteFactory<T>();
            factory.masterNodeList = this;
            factory.newRoutesFromSpecs(routes);
        }
    }
}
