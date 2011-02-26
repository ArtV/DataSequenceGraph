using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public abstract class Route
    {
        public abstract IList<Node> connectedNodes { get; }
        public abstract IEnumerable<Node> requisiteNodes { get; }

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

            IEnumerable<Node> myFirstTwoNodes = this.connectedNodes.Take(2);
            return (myFirstTwoNodes.ElementAt(0) == firstTwoNodes.ElementAt(0) &&
                myFirstTwoNodes.ElementAt(1) == firstTwoNodes.ElementAt(1));
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
                    return criterion.routeSoFar.meetsRequisites(requisiteNodes);
                }
            }
        }

        public bool meetsRequisites(IEnumerable<Node> requisiteNodes)
        {
            var seq = connectedNodes.GetEnumerator(); 
            int numRequisitesMatched = 0;
            int sequenceIndex = 0;
            while (seq.MoveNext())
            {
                IEnumerable<Node> requisiteNodesMatching = requisiteNodes.Where(node =>
                    node == seq.Current);
                numRequisitesMatched += requisiteNodesMatching.Count();
                sequenceIndex++;
            }
            return (numRequisitesMatched == requisiteNodes.Count());
        }

        public EdgeRoute findNextEdgeToFollow()
        {
            if (lastNode.kind == NodeKind.GateNode)
            {
                return lastNode.OutgoingEdges.First();
            }
            IEnumerable<EdgeRoute> edgesToUnvisitedNodesOrStart = lastNode.OutgoingEdges.Where(route =>
                route.edge.link.to == startNode ||
                !connectedNodes.Any(prevNode => prevNode.SequenceNumber == route.edge.link.to.SequenceNumber));                
            var reqIndexAndEdge = findEarliestMatchOfRequisites(edgesToUnvisitedNodesOrStart);
            int earliestRequisiteIndex = reqIndexAndEdge.Item1;
            EdgeRoute selectedRoute;
            if (earliestRequisiteIndex == -1)
            {                
                throw new InvalidOperationException("no earliest requisite found at node " +
                    lastNode.SequenceNumber + " for the route starting at " + startNode.SequenceNumber);
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
            EdgeRoute matchingRoute = null; 
            foreach (EdgeRoute route in edgesToUnvisitedNodesOrStart)
            {
                int indexOfRequisiteMatch = findNode(route.edge.requisiteNodes);
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
        
        public int findNode(IEnumerable<Node> otherNodes)
        {
            int counter = 0;
            foreach (Node node in connectedNodes)
            {
                if (otherNodes.Any(otherNode => otherNode == node))
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }

    }
}
