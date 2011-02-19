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
    }
}
