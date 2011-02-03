using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class OneNodeRoute<T> : Route<T>
    {
        private Node<T> node { get; set; }

        public override Node<T> startNode
        {
            get 
            {
                return node;
            }
        }

        public override IEnumerable<Node<T>> connectedNodes
        {
            get 
            {
                yield return node;
            }
        }

        public override IEnumerable<DirectedPair<T>> requisiteLinks
        {
            get 
            {
                yield return new DirectedPair<T>()
                {
                    from = new NullNode<T>(),
                    to = new NullNode<T>()
                };
            }
        }

        public override Route<T> startRoute
        {
            get 
            {
                throw new NotImplementedException();
            }
        }

        public OneNodeRoute(Node<T> node)
        {
            this.node = node;
        }
    }
}
