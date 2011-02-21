using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteCriterion<T>
    {
        public IEnumerable<T> desiredSequence { get; set; }
        public Route routeSoFar { get; set; }
    }
}
