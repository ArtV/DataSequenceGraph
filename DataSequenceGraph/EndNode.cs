using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EndNode<T> : Node<T>
    {
        public DataChunk<T> sourceDataChunk { get; private set; }

        public EndNode(int SequenceNumber,DataChunk<T> srcDataChunk) : base(SequenceNumber)
        {
            this.sourceDataChunk = srcDataChunk;
        }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.EndNode;
            }
        }
    }
}
