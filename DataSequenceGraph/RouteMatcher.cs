using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public interface RouteMatcher
    {
        // criterion must be RouteCriterion<T> where T is the type of the values in the nodes in the route
        bool prefixMatches(Route candidateRoute, object criterion);
        bool meetsRequisites(Route candidateRoute, IEnumerable<DirectedPair> requisiteLinks);
    }
}
