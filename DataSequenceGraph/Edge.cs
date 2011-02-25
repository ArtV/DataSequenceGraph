using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Edge
    {
        public DirectedPair link { get; set; }

        public IEnumerable<Node> requisiteNodes { get; set; }

        public Edge()
        {
            this.requisiteNodes = new List<Node>();
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Edge p = obj as Edge;
            if ((System.Object)p == null)
            {
                return false;
            }

            return link.Equals(p.link);
        }

        public bool Equals(Edge p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return link.Equals(p.link);
        }

        public override int GetHashCode()
        {
            return link.GetHashCode();
        }
    }
}
