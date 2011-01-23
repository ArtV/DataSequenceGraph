using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class ValueNode<T> : Node
    {
        public T Value { get; private set; }
        public int SequenceNumber { get; private set; }

        public ValueNode(T newValue,int sequenceNumber)
        {
            Value = newValue;
            SequenceNumber = sequenceNumber;
        }
    }
}
