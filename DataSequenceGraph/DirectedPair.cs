using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DirectedPair<T>
    {
        public Node<T> from { get; set; }
        public Node<T> to { get; set; }
    }
}
