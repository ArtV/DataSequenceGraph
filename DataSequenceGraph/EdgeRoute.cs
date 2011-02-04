using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute<T> : Route<T>
    {
        public Edge<T> edge { get; private set; }

        public override Node<T> startNode 
        {
            get
            {
                return connectedNodes.ElementAt(0);
            }
        }
        public override Route<T> startRoute
        {
            get
            {
                return this;
            }
        }
        public override IEnumerable<Node<T>> connectedNodes
        {
            get
            {
                return new List<Node<T>>() 
                {
                    edge.link.from, edge.link.to
                };
            }
        }
        public override IEnumerable<DirectedPair<T>> requisiteLinks
        {
            get
            {
                yield return edge.requisiteLink;
            }
        }

        public EdgeRoute(Edge<T> baseNodes)
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
