using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Node
    {
        public IEnumerable<Route> OutgoingRoutes { get; private set; }
        public int SequenceNumber { get; private set; }

        public Node(int SequenceNumber)
        {
            OutgoingRoutes = new List<Route>();
            this.SequenceNumber = SequenceNumber;
        }

        public void AddOutgoingRoute(Route route)
        {
            OutgoingRoutes = OutgoingRoutes.Concat(new List<Route>() { route });
        }
    }
}
