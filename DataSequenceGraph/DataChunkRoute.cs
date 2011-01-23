using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> SourceData { get; private set; }
        public StartNode InitialNode { get; private set; }
        public MasterNodeList<T> nodeList { get; private set; }

        public DataChunkRoute(IEnumerable<T> sourceData,MasterNodeList<T> nodeList)
        {
            this.SourceData = sourceData;
            this.nodeList = nodeList;

            this.InitialNode = nodeList.newStartNode();
        }
    }
}
