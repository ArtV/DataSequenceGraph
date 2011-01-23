using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class ValueNode<T> : Node
    {
        public T Value { get; private set; }

        public ValueNode(T newValue,int sequenceNumber): base(sequenceNumber)
        {
            Value = newValue;
        }
    }
}
