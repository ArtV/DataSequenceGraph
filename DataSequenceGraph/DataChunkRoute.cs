using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T> : Route<T>
    {
        public IEnumerable<T> dataChunk
        {
            get
            {
                foreach (ValueNode<T> node in chunkRoute.connectedNodes.OfType<ValueNode<T>>().Distinct())
                {
                    yield return node.Value;
                }
            }
        }

        private Route<T> chunkRoute { get; set; }

        public override Node<T> startNode
        {
            get 
            {
                return chunkRoute.startNode;
            }
        }

        public override IEnumerable<Node<T>> connectedNodes
        {
            get 
            {
                return chunkRoute.connectedNodes;
            }
        }

        public override IEnumerable<DirectedPair<T>> requisiteLinks
        {
            get 
            {
                return chunkRoute.requisiteLinks;
            }
        }

        public override Route<T> startRoute
        {
            get 
            {
                return chunkRoute.startRoute;
            }
        }

        public DataChunkRoute(StartNode<T> startNode)
        {
            this.chunkRoute = new RouteFactory<T>().newRouteFromNode(startNode);
        }

        public void appendEdge(EdgeRoute<T> edge)
        {
            this.chunkRoute = new RouteFactory<T>().newRouteFromConnectedRoutes(this.chunkRoute, edge);
        }

        public StartNode<T> getFirstNode()
        {
            return this.chunkRoute.connectedNodes.First() as StartNode<T>;
        }

        public Node<T> getLastNode()
        {
            return this.chunkRoute.connectedNodes.Last();
        }

        public IEnumerable<ValueNode<T>> removeContainedNodes(IEnumerable<ValueNode<T>> otherNodes)
        {
            return otherNodes.Except<ValueNode<T>>(this.chunkRoute.connectedNodes.OfType<ValueNode<T>>());
        }

    }
}
