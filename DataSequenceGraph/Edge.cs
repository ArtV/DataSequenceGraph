using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Edge<T>
    {
        public DirectedPair<T> link { get; set; }

        public DirectedPair<T> requisiteLink { get; set; }

        public Edge()
        {
            this.requisiteLink = new DirectedPair<T>() { 
                from = new NullNode<T>(), to = new NullNode<T>() 
            };
        }        
    }
}
