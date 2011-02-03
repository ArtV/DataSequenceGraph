using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class NullNode<T> : Node<T>
    {
        public NullNode():base(0)
        {
        }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.NullNode;
            }
        }
    }
}
