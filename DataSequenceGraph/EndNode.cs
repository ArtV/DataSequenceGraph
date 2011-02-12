using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EndNode : Node
    {
        public EndNode(int sequenceNumber) : base(sequenceNumber) { }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.EndNode;
            }
        }
    }
}
