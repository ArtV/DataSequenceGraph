using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Route
    {
        public abstract Node startNode { get; }
        public abstract IEnumerable<Node> connectedNodes { get; }
        public abstract IEnumerable<DirectedPair> requisiteLinks { get; }
        public abstract Route startRoute { get; }

        public RouteMatcher matcher { get; set; }
        public Route(RouteMatcher matcher)
        {
            this.matcher = matcher;
        }

        public Node getLastNode()
        {
            return connectedNodes.Last();
        }

        public bool IsNotStartOfAny(IEnumerable<Route> otherRoutes)
        {
            return !otherRoutes.Any(otherRoute => otherRoute.startsWith(startRoute));
        }

        public bool startsWith(Route startingRoute)
        {            
            IEnumerable<Node> firstTwoNodes = startingRoute.connectedNodes.Take(2);
            DirectedPair firstRequisiteLink = startingRoute.requisiteLinks.First<DirectedPair>();

            IEnumerable<Node> myFirstTwoNodes = startRoute.connectedNodes.Take(2);
            DirectedPair myFirstRequisiteLink = startRoute.requisiteLinks.First<DirectedPair>();
            return (myFirstTwoNodes.ElementAt(0) == firstTwoNodes.ElementAt(0) &&
                myFirstTwoNodes.ElementAt(1) == firstTwoNodes.ElementAt(1) &&
                myFirstRequisiteLink.from == firstRequisiteLink.from &&
                myFirstRequisiteLink.to == firstRequisiteLink.to);
        }

        public bool prefixMatches(object criterion)
        {
            return matcher.prefixMatches(this, criterion);
        }

        public bool meetsRequisites(IEnumerable<DirectedPair> requisiteLinks)
        {
            return matcher.meetsRequisites(this, requisiteLinks);
        }

        public EdgeRoute findNextEdgeToFollow()
        {
            Node lastNode = getLastNode();
            int earliestRequisiteIndex = findEarliestIndexOfRequisiteMatches(lastNode);
            EdgeRoute selectedRoute;
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

        public int findEarliestIndexOfRequisiteMatches(Node node)
        {
            int earliestRequisiteIndex = -1;
            foreach (EdgeRoute route in node.OutgoingRoutes)
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
        
        public int positionOfContainedNode(Node otherNode)
        {
            int counter = 0;
            foreach (Node node in connectedNodes)
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
