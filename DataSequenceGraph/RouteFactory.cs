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
            EdgeRoute newRoute = new EdgeRoute(new RouteMatcherImpl<T>(),baseNodes);
            baseNodes.link.from.AddOutgoingRoute(newRoute);
            return newRoute;
        }

        public Route newRouteFromConnectedRoutes(Route firstRoute, Route secondRoute)
        {
            Route newRoute = new CompositeRoute(new RouteMatcherImpl<T>(),new List<Route>() { firstRoute, secondRoute });
            return newRoute;
        }

        public Route newRouteFromNode(Node node)
        {
            Route newRoute = new OneNodeRoute(new RouteMatcherImpl<T>(), node);
            newRoute.matcher = new RouteMatcherImpl<T>();
            return newRoute;
        }

        public IEnumerable<EdgeRoute> newRoutesFromSpecs(IEnumerable<EdgeRouteSpec> specs)
        {
            return specs.Select(spec => newRouteFromSpec(spec));
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

        public RouteMatcher getMatcher()
        {
            return new RouteMatcherImpl<T>();
        }

        public DataChunkRoute<T> newDataChunkRoute(StartNode startNode)
        {
            return new DataChunkRoute<T>(this, startNode);
        }
    }
}
