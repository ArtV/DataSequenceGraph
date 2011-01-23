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

        public Route(Node<T> from, Node<T> to)
        {
            connectedNodes = new List<Node<T>>()
            {
                from, to
            };            
        }

        private Route(IEnumerable<Node<T>> connectedNodes)
        {
            this.connectedNodes = connectedNodes;
        }

        public static Route<T> newRouteBetween(Node<T> from, Node<T> to)
        {
            Route<T> newRoute = new Route<T>(from, to);
            from.AddOutgoingRoute(newRoute);
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
