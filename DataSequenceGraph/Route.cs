using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Route<T>
    {
        public Node<T> startNode 
        {
            get
            {
                return connectedNodes.ElementAt(0);
            }
        }
        public IEnumerable<Node<T>> connectedNodes
        {
            get;
            private set;
        }
        public IEnumerable<Edge<T>> requisiteEdges
        {
            get;
            private set;
        }

        public Route(Edge<T> baseNodes,IEnumerable<Edge<T>> requisiteEdges)
        {
            connectedNodes = new List<Node<T>>()
            {
                baseNodes.from, baseNodes.to
            };
            this.requisiteEdges = requisiteEdges;
        }

        private Route(IEnumerable<Node<T>> connectedNodes)
        {
            this.connectedNodes = connectedNodes;
        }

        public static Route<T> newRouteBetween(Edge<T> baseNodes,IEnumerable<Edge<T>> requisiteEdges)
        {
            Route<T> newRoute = new Route<T>(baseNodes, requisiteEdges);
            baseNodes.from.AddOutgoingRoute(newRoute);
            return newRoute;
        }

        public bool matches(RouteCriterion<T> criterion)
        {
            IEnumerable<ValueNode<T>> routeValueNodes = connectedNodes.OfType<ValueNode<T>>();
            int desiredCount = criterion.desiredSequence.Count();
            if (routeValueNodes.Count() < desiredCount)
            {
                return false;
            }
            else
            {
                IEnumerable<T> routeValues = routeValueNodes.Take(desiredCount).Select(node => node.Value);
                return routeValues.SequenceEqual(criterion.desiredSequence);
            }
        }
    }
}
