using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class StartNode<T> : Node<T>
    {
        public override bool isStartNode
        {
            get
            {
                return true;
            }
        }

        public StartNode(int SequenceNumber) : base(SequenceNumber)
        {
        }
    }
}
