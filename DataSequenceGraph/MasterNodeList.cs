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

        public IEnumerable<NodeSpec<T>> AllNodeSpecs
        {
            get
            {
                return nodeList.Select(node => node.ToNodeSpec());
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

        public StartNode<T> newStartNode(DataChunk<T> sourceDataChunk)
        {
            StartNode<T> newNode = new StartNode<T>(nodeList.Count,sourceDataChunk);
            nodeList.Add(newNode);
            return newNode;
        }

        public EndNode<T> newEndNode(DataChunk<T> sourceDataChunk)
        {
            EndNode<T> newNode = new EndNode<T>(nodeList.Count, sourceDataChunk);
            nodeList.Add(newNode);
            return newNode;
        }
        
        public IEnumerable<IEnumerable<T>> produceDataChunks()
        {
            DataChunkRoute<T> chunkRoute;
            foreach (StartNode<T> node in nodeList.OfType<StartNode<T>>())
            {
                chunkRoute = new DataChunkRoute<T>(node);
                chunkRoute.followToEnd();
                yield return chunkRoute.dataChunk;
            }
        } 

        public void reloadNodesFromSpecs(IEnumerable<NodeSpec<T>> specs)
        {
            foreach (NodeSpec<T> spec in specs)
            {
                switch (spec.kind)
                {
                    case NodeKind.StartNode:
                        newStartNode(null);
                        break;
                    case NodeKind.EndNode:
                        newEndNode(null);
                        break;
                    case NodeKind.NullNode:
                        break;
                    case NodeKind.ValueNode:
                        newValueNodeFromValue(spec.Value);
                        break;
                }
            }
        }
    }
}
