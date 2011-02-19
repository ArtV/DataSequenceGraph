using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public enum NodeKind { GateNode, ValueNode, NullNode }

    public abstract class Node
    {
        public IEnumerable<EdgeRoute> OutgoingEdges { get; private set; }
        public int SequenceNumber { get; private set; }

        public abstract NodeKind kind { get; }

        public Node(int SequenceNumber)
        {
            OutgoingEdges = new List<EdgeRoute>();
            this.SequenceNumber = SequenceNumber;
        }

        public void AddOutgoingEdge(EdgeRoute route)
        {
            OutgoingEdges = OutgoingEdges.Concat(new List<EdgeRoute>() { route });
        }

        public virtual NodeSpec ToNodeSpec()
        {
            return new NodeSpec() { kind = kind, SequenceNumber = SequenceNumber };
        }
    }
}
