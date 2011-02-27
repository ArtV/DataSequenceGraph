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
        public int ReqFromSequenceNumber { get; set; }
        public int ReqToSequenceNumber { get; set; }
    }
}
