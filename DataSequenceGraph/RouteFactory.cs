using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteFactory<T>
    {
        public MasterNodeList<T> masterNodeList { get; set; }

        public EdgeRoute<T> newRouteFromEdge(Edge<T> baseNodes)
        {
            EdgeRoute<T> newRoute = new EdgeRoute<T>(baseNodes);
            baseNodes.link.from.AddOutgoingRoute(newRoute);
            return newRoute;
        }

        public Route<T> newRouteFromConnectedRoutes(Route<T> firstRoute, Route<T> secondRoute)
        {
            Route<T> newRoute = new CompositeRoute<T>(new List<Route<T>>() { firstRoute, secondRoute });
            return newRoute;
        }

        public Route<T> newRouteFromNode(Node<T> node)
        {
            return new OneNodeRoute<T>(node);
        }

        public IEnumerable<EdgeRoute<T>> newRoutesFromSpecs(IEnumerable<EdgeRouteSpec> specs)
        {
            return specs.Select(spec => newRouteFromSpec(spec));
        }

        public EdgeRoute<T> newRouteFromSpec(EdgeRouteSpec spec)
        {
            if (masterNodeList == null)
            {
                throw new InvalidOperationException("masterNodeList must be set to create routes from specs");
            }
            Edge<T> specEdge = new Edge<T>()
            {
                link = new DirectedPair<T>
                {
                    from = masterNodeList.nodeByNumber(spec.FromNumber),
                    to = masterNodeList.nodeByNumber(spec.ToNumber)
                }
            };
            if (spec.RequisiteFromNumber >= 0 && spec.RequisiteToNumber >= 0)
            {
                specEdge.requisiteLink = new DirectedPair<T>()
                {
                    from = masterNodeList.nodeByNumber(spec.RequisiteFromNumber),
                    to = masterNodeList.nodeByNumber(spec.RequisiteToNumber)
                };
            }
            return newRouteFromEdge(specEdge);
        }
    }
}
