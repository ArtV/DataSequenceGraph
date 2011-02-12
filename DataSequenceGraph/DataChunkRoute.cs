using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T> : Route
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

        private Route chunkRoute { get; set; }
        private RouteFactory<T> routeFactory { get; set; }

        public override Node startNode
        {
            get 
            {
                return chunkRoute.startNode;
            }
        }

        public override IEnumerable<Node> connectedNodes
        {
            get 
            {
                return chunkRoute.connectedNodes;
            }
        }

        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get 
            {
                return chunkRoute.requisiteLinks;
            }
        }

        public override Route startRoute
        {
            get 
            {
                return chunkRoute.startRoute;
            }
        }

        internal DataChunkRoute(RouteFactory<T> routeFactory, StartNode startNode) : base(routeFactory.getMatcher())
        {
            this.routeFactory = routeFactory;
            this.chunkRoute = routeFactory.newRouteFromNode(startNode);
        }

        public void appendEdge(EdgeRoute edge)
        {
            this.chunkRoute = routeFactory.newRouteFromConnectedRoutes(this.chunkRoute, edge);
        }

        public StartNode getFirstNode()
        {
            return this.chunkRoute.connectedNodes.First() as StartNode;
        }

        public IEnumerable<ValueNode<T>> removeContainedNodes(IEnumerable<ValueNode<T>> otherNodes)
        {
            return otherNodes.Except<ValueNode<T>>(this.chunkRoute.connectedNodes.OfType<ValueNode<T>>());
        }

        public void followToEnd()
        {
            EdgeRoute nextRoute;
            while (!(getLastNode() is EndNode))
            {
                nextRoute = findEdgeAfterLast();
                appendEdge(nextRoute);
            }
        }

    }
}
