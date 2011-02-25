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
                return _connectedNodes.ToList().AsReadOnly();
            }
        }

        private IEnumerable<Node> _connectedNodes
        {
            get
            {
                if (edge.link.from.kind != NodeKind.NullNode)
                {
                    yield return edge.link.from;
                }
                if (edge.link.to.kind != NodeKind.NullNode)
                {
                    yield return edge.link.to;
                }
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
