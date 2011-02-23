using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class CompositeRoute : Route
    {
        private Route _startRoute { get; set; }
        private IList<Node> _connectedNodes { get; set; }
        private IEnumerable<Node> _requisiteNodes { get; set; }

        internal CompositeRoute(IEnumerable<Route> componentRoutes)
        {
            this._startRoute = componentRoutes.First();
            this._connectedNodes = new List<Node>();
            this._requisiteNodes = Enumerable.Empty<Node>();
            foreach(Route route in componentRoutes)
            {
                this._connectedNodes = this._connectedNodes.Concat(route.connectedNodes).Distinct().ToList().AsReadOnly();
                if (!meetsRequisites(route.requisiteNodes))
                {
                    this._requisiteNodes = this._requisiteNodes.Concat(route.requisiteNodes);
                }
            }
        }

        public override IList<Node> connectedNodes
        {
            get
            {
                return _connectedNodes;
            }
        }

        public override IEnumerable<Node> requisiteNodes
        {
            get
            {
                return _requisiteNodes;
            }
        }
    }
}
