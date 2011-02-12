using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class StartNode : Node
    {
        public StartNode(int sequenceNumber) : base(sequenceNumber) { }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.StartNode;
            }
        }
    }
}
