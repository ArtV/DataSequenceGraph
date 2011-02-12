using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class RouteMatcherImpl<T> : RouteMatcher
    {
        public bool prefixMatches(Route candidateRoute,object criterionArg)
        {
            RouteCriterion<T> criterion = criterionArg as RouteCriterion<T>;
            IEnumerable<ValueNode<T>> routeValueNodes = candidateRoute.connectedNodes.OfType<ValueNode<T>>();
            int desiredCount = criterion.desiredSequence.Count();
            DataChunkRoute<T> routeInProgress = criterion.routeSoFar;
            if (routeValueNodes.Count() < desiredCount)
            {
                return false;
            }
            else
            {
                IEnumerable<T> routeValues = routeValueNodes.Take(desiredCount).Select(node => node.Value);
                if (!routeValues.SequenceEqual(criterion.desiredSequence))
                {
                    return false;
                }
                else
                {
                    return criterion.routeSoFar.meetsRequisites(candidateRoute.requisiteLinks);
                }
            }
        }

        public bool meetsRequisites(Route candidateRoute, IEnumerable<DirectedPair> requisiteLinks)
        {
            IEnumerable<DirectedPair> requisiteLinksNoNulls = requisiteLinks.Where(
                link => link.from.kind != NodeKind.NullNode && link.to.kind != NodeKind.NullNode);
            var seq = candidateRoute.connectedNodes.GetEnumerator();
            int numRequisitesMatched = 0;
            for (int sequenceIndex = 0; sequenceIndex <= candidateRoute.connectedNodes.Count() - 1;
                sequenceIndex++)
            {
                if (!seq.MoveNext())
                {
                    break;
                }
                IEnumerable<DirectedPair> requisiteLinksFrom = requisiteLinksNoNulls.Where(link =>
                    link.from == seq.Current);
                foreach (DirectedPair link in requisiteLinksFrom)
                {
                    if (candidateRoute.connectedNodes.ElementAt(sequenceIndex + 1) == link.to)
                    {
                        numRequisitesMatched++;
                    }
                }
            }
            return (numRequisitesMatched == requisiteLinksNoNulls.Count());
        }
    }
}
