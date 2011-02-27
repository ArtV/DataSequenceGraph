using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Edge
    {
        public DirectedPair link { get; set; }

        public DirectedPair requisiteLink { get; set; }

        public Edge()
        {
            this.requisiteLink = new DirectedPair() { 
                from = NullNode.o, to = NullNode.o
            };
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

            return (link.Equals(p.link) && requisiteLink.Equals(p.requisiteLink));
        }

        public bool Equals(Edge p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (link.Equals(p.link) && requisiteLink.Equals(p.requisiteLink));
        }

        public override int GetHashCode()
        {
            return (link.GetHashCode() ^ requisiteLink.GetHashCode());
        }
    }
}
