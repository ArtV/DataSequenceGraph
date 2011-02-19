using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class NullNode : Node
    {
        public static readonly NullNode o = new NullNode();

        private NullNode():base(-1)
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
