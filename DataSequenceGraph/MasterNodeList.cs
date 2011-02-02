using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node<T>> nodeList = new List<Node<T>>();

        public IEnumerable<Node<T>> AllNodes
        {
            get
            {
                return nodeList;
            }
        }

        public IEnumerable<ValueNode<T>> getValueNodesByValue(T desiredValue)
        {
            return nodeList.OfType<ValueNode<T>>().Where(node => node.Value.Equals(desiredValue));
        }

        public ValueNode<T> newValueNodeFromValue(T newValue)
        {
            ValueNode<T> newNode = new ValueNode<T>(newValue, nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }

        public Node<T> nodeByNumber(int index)
        {
            return nodeList.ElementAt<Node<T>>(index);
        }

        public StartNode<T> newStartNode()
        {
            StartNode<T> newNode = new StartNode<T>(nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }

        public EndNode<T> newEndNode()
        {
            EndNode<T> newNode = new EndNode<T>(nodeList.Count);
            nodeList.Add(newNode);
            return newNode;
        }
    }
}
