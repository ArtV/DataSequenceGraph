using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute : Route
    {
        public Edge edge { get; private set; }

        public override IList<Node> connectedNodes
        {
            get
            {
                return _connectedNodes;
            }
        }

        private IList<Node> _connectedNodes
        {
            get
            {
                List<Node> retNodes = new List<Node>(2);
                if (edge.link.from.kind != NodeKind.NullNode)
                {
                    retNodes.Add(edge.link.from);
                }
                if (edge.link.to.kind != NodeKind.NullNode)
                {
                    retNodes.Add(edge.link.to);
                }
                return retNodes.AsReadOnly();
            }
        }

        public override IEnumerable<Node> requisiteNodes
        {
            get
            {
                return edge.requisiteNodes;
            }
        }

        internal EdgeRoute(Edge baseNodes)
        {
            this.edge = baseNodes;
        }

        public EdgeRouteSpec ToEdgeRouteSpec()
        {
            int[] requisNodes = edge.requisiteNodes.Select(node => node.SequenceNumber).ToArray();
            EdgeRouteSpec retSpec = new EdgeRouteSpec()
            {
                FromNumber = edge.link.from.SequenceNumber,
                ToNumber = edge.link.to.SequenceNumber,
                RequisiteNumbers = requisNodes
            };
            return retSpec;
        }
    }
}
