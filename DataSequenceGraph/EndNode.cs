using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EndNode<T> : Node<T>
    {
        public EndNode(int SequenceNumber) : base(SequenceNumber)
        {
        }
    }
}
