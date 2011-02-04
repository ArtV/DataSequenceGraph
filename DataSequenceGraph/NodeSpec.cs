using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class NodeSpec<T>
    {
        public NodeKind kind { get; set; }
        public T Value { get; set; }
    }
}
