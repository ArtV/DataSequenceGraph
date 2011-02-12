using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DirectedPair
    {
        public Node from { get; set; }
        public Node to { get; set; }

        public bool isBetweenValidNodes()
        {
            return (from.kind != NodeKind.NullNode && to.kind != NodeKind.NullNode);
        }
    }
}
