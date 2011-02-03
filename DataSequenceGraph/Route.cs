using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Route<T>
    {
        public abstract Node<T> startNode { get; }
        public abstract IEnumerable<Node<T>> connectedNodes { get; }
        public abstract IEnumerable<DirectedPair<T>> requisiteLinks { get; }
        public abstract Route<T> startRoute { get; }

        public Node<T> getLastNode()
        {
            return connectedNodes.Last();
        }

        public bool IsNotStartOfAny(IEnumerable<Route<T>> otherRoutes)
        {
            return !otherRoutes.Any(otherRoute => otherRoute.startsWith(startRoute));
        }

        public bool startsWith(Route<T> startingRoute)
        {            
            IEnumerable<Node<T>> firstTwoNodes = startingRoute.connectedNodes.Take(2);
            DirectedPair<T> firstRequisiteLink = startingRoute.requisiteLinks.First<DirectedPair<T>>();

            IEnumerable<Node<T>> myFirstTwoNodes = startRoute.connectedNodes.Take(2);
            DirectedPair<T> myFirstRequisiteLink = startRoute.requisiteLinks.First<DirectedPair<T>>();
            return (myFirstTwoNodes.ElementAt(0) == firstTwoNodes.ElementAt(0) &&
                myFirstTwoNodes.ElementAt(1) == firstTwoNodes.ElementAt(1) &&
                myFirstRequisiteLink.from == firstRequisiteLink.from &&
                myFirstRequisiteLink.to == firstRequisiteLink.to);
        }

        public bool prefixMatches(RouteCriterion<T> criterion)
        {
            IEnumerable<ValueNode<T>> routeValueNodes = connectedNodes.OfType<ValueNode<T>>();
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
                    return criterion.routeSoFar.meetsRequisites(requisiteLinks);
                }
            }
        }

        public bool meetsRequisites(IEnumerable<DirectedPair<T>> requisiteLinks)
        {
            IEnumerable<DirectedPair<T>> requisiteLinksNoNulls = requisiteLinks.Where(
                link => link.from.kind != NodeKind.NullNode && link.to.kind != NodeKind.NullNode);
            var seq = this.connectedNodes.GetEnumerator();
            int numRequisitesMatched = 0;
            for (int sequenceIndex = 0; sequenceIndex <= this.connectedNodes.Count() - 1;
                sequenceIndex++)
            {
                if (!seq.MoveNext())
                {
                    break;
                }
                IEnumerable<DirectedPair<T>> requisiteLinksFrom = requisiteLinksNoNulls.Where(link =>
                    link.from == seq.Current);
                foreach (DirectedPair<T> link in requisiteLinksFrom)
                {
                    if (this.connectedNodes.ElementAt(sequenceIndex + 1) == link.to)
                    {
                        numRequisitesMatched++;
                    }
                }
            }
            return (numRequisitesMatched == requisiteLinksNoNulls.Count());
        }

        public EdgeRoute<T> findEdgeAfterLast()
        {
            Node<T> lastNode = getLastNode();
            int earliestRequisiteIndex = findEarliestRequisiteMatchIndex(lastNode);
            EdgeRoute<T> selectedRoute;
            if (earliestRequisiteIndex == -1)
            {
                selectedRoute = lastNode.OutgoingRoutes.First();
            }
            else
            {
                selectedRoute = lastNode.OutgoingRoutes.First(route =>
                    route.edge.requisiteLink.from == connectedNodes.ElementAt(earliestRequisiteIndex));
            }
            return selectedRoute;
        }

        public int findEarliestRequisiteMatchIndex(Node<T> node)
        {
            int earliestRequisiteIndex = -1;
            foreach (EdgeRoute<T> route in node.OutgoingRoutes)
            {
                int indexOfRequisiteMatch = positionOfContainedNode(route.edge.requisiteLink.from);
                if (earliestRequisiteIndex == -1)
                {
                    earliestRequisiteIndex = indexOfRequisiteMatch;
                }
                else if (indexOfRequisiteMatch != -1)
                {
                    earliestRequisiteIndex = Math.Min(earliestRequisiteIndex, indexOfRequisiteMatch);
                }
            }
            return earliestRequisiteIndex;
        }
        
        public int positionOfContainedNode(Node<T> otherNode)
        {
            int counter = 0;
            foreach (Node<T> node in connectedNodes)
            {
                if (node == otherNode)
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }

    }
}
