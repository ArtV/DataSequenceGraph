using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class OneNodeRoute : Route
    {
        private Node node { get; set; }

        public override Node startNode
        {
            get 
            {
                return node;
            }
        }

        public override IEnumerable<Node> connectedNodes
        {
            get 
            {
                yield return node;
            }
        }

        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get 
            {
                yield return new DirectedPair()
                {
                    from = new NullNode(),
                    to = new NullNode()
                };
            }
        }

        public override Route startRoute
        {
            get 
            {
                throw new NotImplementedException();
            }
        }

        internal OneNodeRoute(RouteMatcher matcher, Node node) : base(matcher)
        {
            this.node = node;
        }
    }
}
