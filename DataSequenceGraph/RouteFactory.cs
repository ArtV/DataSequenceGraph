using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteFactory<T>
    {
        public MasterNodeList<T> masterNodeList { get; set; }

        public EdgeRoute newRouteFromEdge(Edge baseNodes)
        {
            EdgeRoute newRoute = new EdgeRoute(baseNodes);
            if (baseNodes.link.isBetweenValidNodes())
            {
                baseNodes.link.from.AddOutgoingEdge(newRoute);
            }
            return newRoute;
        }

        public Route newRouteFromConnectedRoutes(Route firstRoute, Route secondRoute)
        {
            Route newRoute = new CompositeRoute(new List<Route>() { firstRoute, secondRoute });
            return newRoute;
        }

        public Route newRouteFromNode(Node node)
        {
            Edge newEdge = new Edge()
            {
                link = new DirectedPair()
                {
                    from = node,
                    to = NullNode.o
                }
            };
            return newRouteFromEdge(newEdge);
        }
        
        public void newRoutesFromSpecs(IEnumerable<EdgeRouteSpec> specs)
        {
            foreach (EdgeRouteSpec spec in specs)
            {
                newRouteFromSpec(spec);
            }
        }

        public EdgeRoute newRouteFromSpec(EdgeRouteSpec spec)
        {
            if (masterNodeList == null)
            {
                throw new InvalidOperationException("masterNodeList must be set to create routes from specs");
            }
            Edge specEdge = new Edge()
            {
                link = new DirectedPair
                {
                    from = masterNodeList.nodeByNumber(spec.FromNumber),
                    to = masterNodeList.nodeByNumber(spec.ToNumber)
                }
            };
            if (spec.RequisiteFromNumber >= 0 && spec.RequisiteToNumber >= 0)
            {
                specEdge.requisiteLink = new DirectedPair()
                {
                    from = masterNodeList.nodeByNumber(spec.RequisiteFromNumber),
                    to = masterNodeList.nodeByNumber(spec.RequisiteToNumber)
                };
            }
            return newRouteFromEdge(specEdge);
        }

        public DataChunkRoute<T> newDataChunkRoute(GateNode startNode)
        {
            return new DataChunkRoute<T>(this, startNode);
        }
    }
}
