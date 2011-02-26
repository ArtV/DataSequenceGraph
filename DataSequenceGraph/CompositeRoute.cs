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
            List<Node> newNodeList = new List<Node>();
            this._requisiteNodes = Enumerable.Empty<Node>();
            bool isFirstRoute = true;
            foreach(Route route in componentRoutes)
            {
                IEnumerable<Node> newNodes;
                if (isFirstRoute)
                {
                    newNodes = route.connectedNodes;
                }
                else
                {
                    newNodes = route.connectedNodes.Skip(1);
                }
                newNodeList.AddRange(newNodes);
                this._connectedNodes = newNodeList.AsReadOnly();
                if (!meetsRequisites(route.requisiteNodes))
                {
                    this._requisiteNodes = this._requisiteNodes.Concat(route.requisiteNodes);
                }
                isFirstRoute = false;
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
