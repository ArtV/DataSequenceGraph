using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class ValueNode<T> : Node<T>
    {
        public T Value { get; private set; }

        public ValueNode(T newValue,int sequenceNumber): base(sequenceNumber)
        {
            Value = newValue;
        }

        public IEnumerable<Route<T>> findMatchingRoutes(RouteCriterion<T> criterion)
        {
            return OutgoingRoutes.Where(route => route.prefixMatches(criterion));
        }

        public override NodeKind kind
        {
            get 
            {
                return NodeKind.ValueNode;
            }
        }
    }
}
