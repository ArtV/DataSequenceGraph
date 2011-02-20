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

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DirectedPair p = obj as DirectedPair;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (from.Equals(p.from) && to.Equals(p.to));
        }

        public bool Equals(DirectedPair p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (from.Equals(p.from) && to.Equals(p.to));
        }

        public override int GetHashCode()
        {
            return (from.GetHashCode() ^ to.GetHashCode());
        }
    }
}
