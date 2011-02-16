using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute : Route
    {
        public Edge edge { get; private set; }

        public override IEnumerable<Node> connectedNodes
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
        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get
            {
                yield return edge.requisiteLink;
            }
        }

        internal EdgeRoute(Edge baseNodes)
        {
            this.edge = baseNodes;
        }

        public EdgeRouteSpec ToEdgeRouteSpec()
        {
            EdgeRouteSpec retSpec = new EdgeRouteSpec()
            {
                FromNumber = edge.link.from.SequenceNumber,
                ToNumber = edge.link.to.SequenceNumber,
                RequisiteFromNumber = edge.requisiteLink.from.SequenceNumber,
                RequisiteToNumber = edge.requisiteLink.to.SequenceNumber
            };
            return retSpec;
        }
    }
}
