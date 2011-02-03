using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class StartNode<T> : Node<T>
    {
        public DataChunk<T> sourceDataChunk { get; private set; }

        public StartNode(int SequenceNumber,DataChunk<T> sourceDataChunk) : base(SequenceNumber)
        {
            this.sourceDataChunk = sourceDataChunk;
        }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.StartNode;
            }
        }
    }
}
