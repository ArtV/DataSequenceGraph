using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute : Route
    {
        public Edge edge { get; private set; }

        public override Node startNode 
        {
            get
            {
                return connectedNodes.ElementAt(0);
            }
        }
        public override Route startRoute
        {
            get
            {
                return this;
            }
        }
        public override IEnumerable<Node> connectedNodes
        {
            get
            {
                return new List<Node>() 
                {
                    edge.link.from, edge.link.to
                };
            }
        }
        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get
            {
                yield return edge.requisiteLink;
            }
        }

        internal EdgeRoute(RouteMatcher matcher, Edge baseNodes) : base(matcher)
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
