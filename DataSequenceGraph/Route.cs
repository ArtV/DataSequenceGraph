using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Route<T>
    {
        public abstract Node<T> startNode { get; }
        public abstract IEnumerable<Node<T>> connectedNodes { get; }
        public abstract IEnumerable<Edge<T>> requisiteEdges { get; }
        public abstract Route<T> startRoute { get; }

        public bool IsNotStartOfAny(IEnumerable<Route<T>> otherRoutes)
        {
            return !otherRoutes.Any(otherRoute => otherRoute.startsWith(startRoute));
        }

        public bool startsWith(Route<T> startingRoute)
        {            
            IEnumerable<Node<T>> firstTwoNodes = startingRoute.connectedNodes.Take(2);
            Edge<T> firstRequisiteEdge = startingRoute.requisiteEdges.First<Edge<T>>();

            IEnumerable<Node<T>> myFirstTwoNodes = startRoute.connectedNodes.Take(2);
            Edge<T> myFirstRequisiteEdge = startRoute.requisiteEdges.First<Edge<T>>();
            return (myFirstTwoNodes.ElementAt(0) == firstTwoNodes.ElementAt(0) &&
                myFirstTwoNodes.ElementAt(1) == firstTwoNodes.ElementAt(1) &&
                myFirstRequisiteEdge.requisiteEdgeFrom == firstRequisiteEdge.requisiteEdgeFrom &&
                myFirstRequisiteEdge.requisiteEdgeTo == firstRequisiteEdge.requisiteEdgeTo);
        }

        public bool prefixMatches(RouteCriterion<T> criterion)
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
