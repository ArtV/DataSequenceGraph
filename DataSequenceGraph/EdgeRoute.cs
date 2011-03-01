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

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            EdgeRoute p = obj as EdgeRoute;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (edge.Equals(p.edge));
        }

        public bool Equals(EdgeRoute p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (edge.Equals(p.edge));
        }

        public override int GetHashCode()
        {
            return (edge.GetHashCode());
        }
    }
}
