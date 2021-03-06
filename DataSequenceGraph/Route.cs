﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Route
    {
        public abstract IList<Node> connectedNodes { get; }
        public abstract IEnumerable<DirectedPair> requisiteLinks { get; }

        public Node startNode
        {
            get
            {
                return connectedNodes.First();
            }
        }

        public Node lastNode
        {
            get
            {
                return connectedNodes.Last();
            }
        }

        public bool IsNotStartOfAny(IEnumerable<Route> otherRoutes)
        {
            return !otherRoutes.Any(otherRoute => otherRoute.startsWith(this));
        }

        public bool startsWith(Route startingRoute)
        {            
            IEnumerable<Node> firstTwoNodes = startingRoute.connectedNodes.Take(2);
            DirectedPair firstRequisiteLink = startingRoute.requisiteLinks.First<DirectedPair>();

            IEnumerable<Node> myFirstTwoNodes = this.connectedNodes.Take(2);
            DirectedPair myFirstRequisiteLink = this.requisiteLinks.First<DirectedPair>();
            return (myFirstTwoNodes.ElementAt(0) == firstTwoNodes.ElementAt(0) &&
                myFirstTwoNodes.ElementAt(1) == firstTwoNodes.ElementAt(1) &&
                myFirstRequisiteLink.from == firstRequisiteLink.from &&
                myFirstRequisiteLink.to == firstRequisiteLink.to);
        }

        public bool prefixMatches<NodeValType>(RouteCriterion<NodeValType> criterion)
        {
            IEnumerable<ValueNode<NodeValType>> routeValueNodes = connectedNodes.OfType<ValueNode<NodeValType>>();
            int desiredCount = criterion.desiredSequence.Count();
            Route routeInProgress = criterion.routeSoFar;
            if (routeValueNodes.Count() < desiredCount)
            {
                return false;
            }
            else
            {
                IEnumerable<NodeValType> routeValues = routeValueNodes.Take(desiredCount).Select(node => node.Value);
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

        public bool meetsRequisites(IEnumerable<DirectedPair> requisiteLinks)
        {
            IEnumerable<DirectedPair> requisiteLinksNoNulls = requisiteLinks.Where(
                link => link.isBetweenValidNodes());
            var seq = connectedNodes.GetEnumerator(); 
            int numRequisitesMatched = 0;
            int sequenceIndex = 0;
            int nextToLastIndex = connectedNodes.Count - 2;
            while (seq.MoveNext() && sequenceIndex <= nextToLastIndex)
            {
                IEnumerable<DirectedPair> requisiteLinksFrom = requisiteLinksNoNulls.Where(link =>
                    link.from == seq.Current);
                foreach (DirectedPair link in requisiteLinksFrom)
                {
                    if (connectedNodes.ElementAt(sequenceIndex + 1) == link.to)
                    {
                        numRequisitesMatched++;
                    }
                }
                sequenceIndex++;
            }
            return (numRequisitesMatched == requisiteLinksNoNulls.Count());
        }

        public EdgeRoute findNextEdgeToFollow()
        {
            IEnumerable<EdgeRoute> edgesToUnvisitedNodesOrStart = lastNode.OutgoingEdges.Where(route =>
                route.edge.link.to == startNode ||
                !connectedNodes.Any(prevNode => prevNode.SequenceNumber == route.edge.link.to.SequenceNumber));                
            var reqIndexAndEdge = findEarliestMatchOfRequisites(edgesToUnvisitedNodesOrStart);
            int earliestRequisiteIndex = reqIndexAndEdge.Item1;
            EdgeRoute selectedRoute;
            if (earliestRequisiteIndex == -1)
            {                
                selectedRoute = edgesToUnvisitedNodesOrStart.First(route =>
                    route.edge.requisiteLink.from.kind == NodeKind.NullNode);
            }
            else
            {
                selectedRoute = reqIndexAndEdge.Item2;

            }
            return selectedRoute;
        }

        public Tuple<int,EdgeRoute> findEarliestMatchOfRequisites(IEnumerable<EdgeRoute> edgesToUnvisitedNodesOrStart)
        {
            int earliestRequisiteIndex = -1;
            int nextToLastIndex = connectedNodes.Count - 2;
            EdgeRoute matchingRoute = null; 
            foreach (EdgeRoute route in edgesToUnvisitedNodesOrStart)
            {
                int indexOfRequisiteMatch = findNode(route.edge.requisiteLink.from);
                if (indexOfRequisiteMatch > nextToLastIndex ||
                    connectedNodes.ElementAt(indexOfRequisiteMatch + 1) != route.edge.requisiteLink.to)
                {
                    continue;
                }
                if (earliestRequisiteIndex == -1)
                {
                    earliestRequisiteIndex = indexOfRequisiteMatch;
                    matchingRoute = route;
                }
                else if (indexOfRequisiteMatch != -1 && indexOfRequisiteMatch < earliestRequisiteIndex)
                {
                    earliestRequisiteIndex = indexOfRequisiteMatch;
                    matchingRoute = route;
                }
            }
            return Tuple.Create(earliestRequisiteIndex, matchingRoute);
        }
        
        public int findNode(Node otherNode)
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
