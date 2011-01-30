using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public interface Route<T>
    {
        bool prefixMatches(RouteCriterion<T> criterion);
        Node<T> startNode { get; }
        IEnumerable<Node<T>> connectedNodes { get; }
        IEnumerable<Edge<T>> requisiteEdges { get; }
    }
}
