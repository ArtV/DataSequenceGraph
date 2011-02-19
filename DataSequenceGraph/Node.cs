using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public enum NodeKind { GateNode, ValueNode, NullNode }

    public abstract class Node
    {
        public IEnumerable<EdgeRoute> OutgoingRoutes { get; private set; }
        public int SequenceNumber { get; private set; }

        public abstract NodeKind kind { get; }

        public Node(int SequenceNumber)
        {
            OutgoingRoutes = new List<EdgeRoute>();
            this.SequenceNumber = SequenceNumber;
        }

        public void AddOutgoingRoute(EdgeRoute route)
        {
            OutgoingRoutes = OutgoingRoutes.Concat(new List<EdgeRoute>() { route });
        }

        public virtual NodeSpec ToNodeSpec()
        {
            return new NodeSpec() { kind = kind, SequenceNumber = SequenceNumber };
        }
    }
}
