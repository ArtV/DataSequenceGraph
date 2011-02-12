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

        internal DataChunkRoute(RouteFactory<T> routeFactory, StartNode startNode) : base(routeFactory.newMatcher())
        {
            this.routeFactory = routeFactory;
            this.chunkRoute = routeFactory.newRouteFromNode(startNode);
        }

        public void appendEdge(EdgeRoute edge)
        {
            this.chunkRoute = routeFactory.newRouteFromConnectedRoutes(this.chunkRoute, edge);
        }

        public IEnumerable<ValueNode<T>> excludeMyNodesFrom(IEnumerable<ValueNode<T>> otherNodes)
        {
            return otherNodes.Except<ValueNode<T>>(this.chunkRoute.connectedNodes.OfType<ValueNode<T>>());
        }

        public void followToEnd()
        {
            EdgeRoute nextRoute;
            while (!(lastNode.kind == NodeKind.EndNode))
            {
                nextRoute = findNextEdgeToFollow();
                appendEdge(nextRoute);
            }
        }

    }
}
