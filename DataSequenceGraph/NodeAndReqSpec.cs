using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class NodeAndReqSpec
    {
        public NodeSpec fromNode { get; set; }
        public bool insertFrom { get; set; }
        public bool usePrevEdgeAsReq { get; set; }
        public bool useStartEdgeAsReq { get; set; }
        public int reqFromRouteIndex { get; set; }
    }
}
