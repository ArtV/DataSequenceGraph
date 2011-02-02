using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> dataChunk
        {
            get
            {
                foreach (ValueNode<T> node in connectedNodes.OfType<ValueNode<T>>())
                {
                    yield return node.Value;
                }
            }
        }

        private List<Node<T>> connectedNodes { get; set; }

        public DataChunkRoute(IEnumerable<Node<T>> startingNodes)
        {
            this.connectedNodes = startingNodes.ToList();
        }

        public void addNode(Node<T> node)
        {
            this.connectedNodes.Add(node);
        }

        public StartNode<T> getFirstNode()
        {
            return connectedNodes.First() as StartNode<T>;
        }

        public Node<T> getLastNode()
        {
            return connectedNodes.Last();
        }

        public bool meetsRequisites(IEnumerable<DirectedPair<T>> requisiteLinks)
        {
            var seq = connectedNodes.GetEnumerator();
            int numRequisitesMatched = 0;
            for (int sequenceIndex = 0; sequenceIndex <= connectedNodes.Count() - 1;
                sequenceIndex++)
            {
                if (!seq.MoveNext())
                {
                    break;
                }
                IEnumerable<DirectedPair<T>> requisiteLinksFrom = requisiteLinks.Where(link =>
                    link.from == seq.Current);
                foreach (DirectedPair<T> link in requisiteLinksFrom)
                {
                    if (connectedNodes.ElementAt(sequenceIndex + 1) == link.to)
                    {
                        numRequisitesMatched++;
                    }
                }
            }
            return (numRequisitesMatched == requisiteLinks.Count());
        }

        public IEnumerable<ValueNode<T>> removeContainedNodes(IEnumerable<ValueNode<T>> otherNodes)
        {
            return otherNodes.Except<ValueNode<T>>(connectedNodes.OfType<ValueNode<T>>());
        }

        public int positionOfContainedNode(Node<T> otherNode)
        {
            return connectedNodes.FindIndex(connectedNode => connectedNode == otherNode);
        }
    }
}
