using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class CompositeRoute : Route
    {
        private Route _startRoute { get; set; }
        private IEnumerable<Node> _connectedNodes { get; set; }
        private IEnumerable<DirectedPair> _requisiteLinks { get; set; }

        internal CompositeRoute(IEnumerable<Route> componentRoutes)
        {
            this._startRoute = componentRoutes.First();
            this._connectedNodes = Enumerable.Empty<Node>();
            this._requisiteLinks = Enumerable.Empty<DirectedPair>();
            foreach(Route route in componentRoutes)
            {
                this._connectedNodes = this._connectedNodes.Concat(route.connectedNodes).Distinct();
                if (!meetsRequisites(route.requisiteLinks))
                {
                    this._requisiteLinks = this._requisiteLinks.Concat(route.requisiteLinks);
                }
            }
        }

        public override IEnumerable<Node> connectedNodes
        {
            get
            {
                return _connectedNodes;
            }
        }

        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get
            {
                return _requisiteLinks;
            }
        }
    }
}
