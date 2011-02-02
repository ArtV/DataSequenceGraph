using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Node<T>
    {
        public IEnumerable<EdgeRoute<T>> OutgoingRoutes { get; private set; }
        public int SequenceNumber { get; private set; }
        public virtual bool isStartNode
        {
            get
            {
                return false;
            }
        }

        public Node(int SequenceNumber)
        {
            OutgoingRoutes = new List<EdgeRoute<T>>();
            this.SequenceNumber = SequenceNumber;
        }

        public void AddOutgoingRoute(EdgeRoute<T> route)
        {
            OutgoingRoutes = OutgoingRoutes.Concat(new List<EdgeRoute<T>>() { route });
        }
    }
}
