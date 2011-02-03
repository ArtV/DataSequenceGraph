using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class CompositeRoute<T> : Route<T>
    {
        private Route<T> _startRoute { get; set; }
        private IEnumerable<Node<T>> _connectedNodes { get; set; }
        private IEnumerable<DirectedPair<T>> _requisiteLinks { get; set; }

        public CompositeRoute(IEnumerable<Route<T>> componentRoutes)
        {
            this._startRoute = componentRoutes.First();
            this._connectedNodes = Enumerable.Empty<Node<T>>();
            this._requisiteLinks = Enumerable.Empty<DirectedPair<T>>();
            foreach(Route<T> route in componentRoutes)
            {
                this._connectedNodes = this._connectedNodes.Concat(route.connectedNodes).Distinct();
                if (!meetsRequisites(route.requisiteLinks))
                {
                    this._requisiteLinks = this._requisiteLinks.Concat(route.requisiteLinks);
                }
            }
        }

        public override Node<T> startNode
        {
            get
            {
                return _startRoute.startNode;
            }
        }

        public override Route<T> startRoute
        {
            get 
            {
                return _startRoute;
            }
        }

        public override IEnumerable<Node<T>> connectedNodes
        {
            get
            {
                return _connectedNodes;
            }
        }

        public override IEnumerable<DirectedPair<T>> requisiteLinks
        {
            get
            {
                return _requisiteLinks;
            }
        }
    }
}
