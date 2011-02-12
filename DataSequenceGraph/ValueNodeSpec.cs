using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class ValueNodeSpec<T> : NodeSpec
    {
        public T Value { get; set; }
    }
}
