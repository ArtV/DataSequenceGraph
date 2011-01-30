using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class BaseRoute<T>
    {
        public abstract Node<T> startNode { get; }
        public abstract IEnumerable<Node<T>> connectedNodes { get; }
        public abstract IEnumerable<Edge<T>> requisiteEdges { get; }

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
