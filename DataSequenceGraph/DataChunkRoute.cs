using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> SourceData { get; private set; }
        public StartNode<T> InitialNode
        {
            get
            {
                return connectedNodes.ElementAt(0) as StartNode<T>;
            }
        }
        public MasterNodeList<T> nodeList { get; private set; }
        public List<Node<T>> connectedNodes { get; private set; }
        public EndNode<T> finishNode 
        {
            get
            {
                return connectedNodes.Last() as EndNode<T>;
            }
        }
        public bool Done { get; private set; }

        private int sourceDataIndex;
        private Dictionary<IEnumerable<T>, Route<T>> routePrefixDictionary;

        public DataChunkRoute(IEnumerable<T> sourceData,MasterNodeList<T> nodeList)
        {
            this.Done = false;
            this.SourceData = sourceData;
            sourceDataIndex = 0;
            this.nodeList = nodeList;
            this.connectedNodes = new List<Node<T>>()
            {
                nodeList.newStartNode()
            };
            this.routePrefixDictionary = new Dictionary<IEnumerable<T>, Route<T>>();
        }

        public void appendToRoute()
        {
            var nextValue = SourceData.Skip(sourceDataIndex).Take(1);
            if (!routePrefixDictionary.ContainsKey(nextValue))
            {
                ValueNode<T> newValueNode = nodeList.newValueNodeFromValue(nextValue.ElementAt(0));
                connectedNodes.Add(newValueNode);
            }

            sourceDataIndex++;
            if (sourceDataIndex >= SourceData.Count())
            {
                EndNode<T> endNode = nodeList.newEndNode();
                connectedNodes.Add(endNode);
                this.Done = true;
            }
        }
    }
}
