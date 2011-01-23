using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node> nodeList = new List<Node>();

        public IEnumerable<ValueNode<T>> getValueNodes()
        {
            return nodeList.OfType<ValueNode<T>>();
        }

        public ValueNode<T> newValueNodeFromValue(T newValue)
        {
            ValueNode<T> newNode = new ValueNode<T>(newValue, nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }

        public Node nodeByNumber(int index)
        {
            return nodeList.ElementAt<Node>(index);
        }


    }
}
