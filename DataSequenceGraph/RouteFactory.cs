using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteFactory<T>
    {
        public Route<T> newRouteFromEdge(Edge<T> baseNodes)
        {
            Route<T> newRoute = new EdgeRoute<T>(baseNodes);
            baseNodes.from.AddOutgoingRoute(newRoute);
            return newRoute;
        }
    }
}
