using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Edge<T>
    {
        public Node<T> from { get; set; }
        public Node<T> to { get; set; }

        public static IEnumerable<Edge<T>> nodeSequenceToEdges(IEnumerable<Node<T>> nodeSequence)
        {
            // solution from http://stackoverflow.com/questions/577590/pair-wise-iteration-in-c-or-sliding-window-enumerator
            return nodeSequence.Zip(nodeSequence.Skip(1),
                (fromNode, toNode) => new Edge<T>() { from = fromNode, to = toNode } );
        }
    }
}
