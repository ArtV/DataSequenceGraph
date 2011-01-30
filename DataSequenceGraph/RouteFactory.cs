using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteFactory<T>
    {
        public Route<T> newRouteBetween(Edge<T> baseNodes, Edge<T> requisiteEdge)
        {
            Route<T> newRoute = new EdgeRoute<T>(baseNodes, requisiteEdge);
            baseNodes.from.AddOutgoingRoute(newRoute);
            return newRoute;
        }
    }
}
