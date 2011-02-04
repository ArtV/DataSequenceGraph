using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node<T>> nodeList = new List<Node<T>>();

        public IEnumerable<Node<T>> AllNodes
        {
            get
            {
                return nodeList;
            }
        }

        public List<NodeSpec<T>> AllNodeSpecs
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

        public List<Tuple<NodeSpec<T>, IEnumerable<EdgeRouteSpec>>> AllNodeAndRouteSpecs
        {
            get
            {
                List<Tuple<NodeSpec<T>, IEnumerable<EdgeRouteSpec>>> retList = new List<Tuple<NodeSpec<T>, IEnumerable<EdgeRouteSpec>>>();
                foreach (Node<T> node in nodeList)
                {
                    Tuple<NodeSpec<T>, IEnumerable<EdgeRouteSpec>> retElem = 
                        new Tuple<NodeSpec<T>, IEnumerable<EdgeRouteSpec>>(node.ToNodeSpec(), nodeToRoutesSpecs(node));                    
                    retList.Add(retElem);
                }
                return retList;
            }
        }        

        private IEnumerable<EdgeRouteSpec> nodeToRoutesSpecs(Node<T> node)
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

        public Node<T> nodeByNumber(int index)
        {
            return nodeList.ElementAt<Node<T>>(index);
        }

        public StartNode<T> newStartNode(DataChunk<T> sourceDataChunk)
        {
            StartNode<T> newNode = new StartNode<T>(nodeList.Count,sourceDataChunk);
            nodeList.Add(newNode);
            return newNode;
        }

        public EndNode<T> newEndNode(DataChunk<T> sourceDataChunk)
        {
            EndNode<T> newNode = new EndNode<T>(nodeList.Count, sourceDataChunk);
            nodeList.Add(newNode);
            return newNode;
        }
        
        public IEnumerable<IEnumerable<T>> produceDataChunks()
        {
            DataChunkRoute<T> chunkRoute;
            foreach (StartNode<T> node in nodeList.OfType<StartNode<T>>())
            {
                chunkRoute = new DataChunkRoute<T>(node);
                chunkRoute.followToEnd();
                yield return chunkRoute.dataChunk;
            }
        } 

        public void reloadNodesFromSpecs(IEnumerable<NodeSpec<T>> specs)
        {
            foreach (NodeSpec<T> spec in specs)
            {
                switch (spec.kind)
                {
                    case NodeKind.StartNode:
                        newStartNode(null);
                        break;
                    case NodeKind.EndNode:
                        newEndNode(null);
                        break;
                    case NodeKind.NullNode:
                        break;
                    case NodeKind.ValueNode:
                        newValueNodeFromValue(spec.Value);
                        break;
                }
            }
        }

        public void reloadNodesThenRoutesFromSpecs(IEnumerable<NodeSpec<T>> nodes, IEnumerable<EdgeRouteSpec> routes)
        {
            reloadNodesFromSpecs(nodes);
            RouteFactory<T> factory = new RouteFactory<T>();
            factory.masterNodeList = this;
            factory.newRoutesFromSpecs(routes);
        }
    }
}
