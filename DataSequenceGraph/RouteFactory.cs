using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteFactory<T>
    {
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
    }
}
