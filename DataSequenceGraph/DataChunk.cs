using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public interface DataChunk<T>
    {
        IEnumerable<T> sourceData { get; }
    }
}
