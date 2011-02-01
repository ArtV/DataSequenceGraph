using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> SourceData { get; private set; }
        public StartNode<T> InitialNode
        {
            get
            {
                return connectedNodes.ElementAt(0) as StartNode<T>;
            }
        }
        public MasterNodeList<T> nodeList { get; set; }
        public Dictionary<IEnumerable<T>, Route<T>> routePrefixDictionary { get; set; }
        public List<Node<T>> connectedNodes { get; private set; }
        public EndNode<T> finishNode 
        {
            get
            {
                return connectedNodes.Last() as EndNode<T>;
            }
        }
        public bool Done { get; private set; }

        private int sourceDataIndex;
        private List<Edge<T>> addedEdges { get; set; }

        public DataChunkRoute(IEnumerable<T> sourceData,MasterNodeList<T> nodeList,
            Dictionary<IEnumerable<T>, Route<T>> routePrefixDictionary)
        {
            this.Done = false;
            this.SourceData = sourceData;
            this.sourceDataIndex = 0;
            this.nodeList = nodeList;
            this.connectedNodes = new List<Node<T>>()
            {
                nodeList.newStartNode()
            };
            this.routePrefixDictionary = routePrefixDictionary;
            this.addedEdges = new List<Edge<T>>();
        }

        public void appendToRoute()
        {
            var nextValueSequence = SourceData.Skip(sourceDataIndex).Take(1);
            if (nextValueSequence.Count() > 0)
            {
                if (!routePrefixDictionary.ContainsKey(nextValueSequence))
                {
                    T nextValue = nextValueSequence.ElementAt(0);
                    IEnumerable<ValueNode<T>> matchingUnusedNodes =
                        nodeList.getValueNodesByValue(nextValue).Except(connectedNodes.OfType<ValueNode<T>>());
                    if (matchingUnusedNodes.Count() == 0)
                    {
                        makeNewNode(nextValue);
                    }
                    else
                    {
                        appendEdgeTo(matchingUnusedNodes.ElementAt(0));
                    }
                }
            }

            sourceDataIndex++;
            if (sourceDataIndex >= SourceData.Count())
            {
                EndNode<T> endNode = nodeList.newEndNode();
                connectedNodes.Add(endNode);
                this.Done = true;
            }
        }

        private void makeNewNode(T newValue)
        {
            ValueNode<T> newValueNode = nodeList.newValueNodeFromValue(newValue);
            appendEdgeTo(newValueNode);
        }

        private void appendEdgeTo(Node<T> nextNode)
        {
            Node<T> previousLastNode = connectedNodes[connectedNodes.Count - 1];
            Edge<T> newEdge;
            if (previousLastNode.GetType() == typeof(StartNode<T>))
            {
                newEdge =
                    new Edge<T>()
                    {
                        from = previousLastNode,
                        to = nextNode
                    };
            }
            else
            {
                Edge<T> lastAddedEdge = addedEdges[addedEdges.Count - 1];
                newEdge =
                    new Edge<T>()
                    {
                        from = previousLastNode,
                        to = nextNode,
                        requisiteEdgeFrom = lastAddedEdge.from,
                        requisiteEdgeTo = lastAddedEdge.to
                    };
            }
            addedEdges.Add(newEdge);
            Route<T> newRoute = new RouteFactory<T>().newRouteFromEdge(newEdge);
            previousLastNode.AddOutgoingRoute(newRoute);
            connectedNodes.Add(nextNode);
        }
    }
}
