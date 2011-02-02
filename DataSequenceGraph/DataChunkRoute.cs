using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> SourceData 
        { 
            get
            {
                return this.sourceDataChunk.sourceData;
            }
        }
        public StartNode<T> InitialNode
        {
            get
            {
                return connectedNodes.ElementAt(0) as StartNode<T>;
            }
        }
        public MasterNodeList<T> nodeList { get; set; }
        public Dictionary<Node<T>, List<Route<T>>> nodeRoutesDictionary { get; set; }
        public List<Node<T>> connectedNodes { get; private set; }
        public EndNode<T> finishNode 
        {
            get
            {
                return connectedNodes.Last() as EndNode<T>;
            }
        }
        public bool Done { get; private set; }

        private int sourceDataIndex;
        private List<DirectedPair<T>> addedLinks { get; set; }
        private DataChunk<T> sourceDataChunk { get; set; }

        public DataChunkRoute(DataChunk<T> sourceDataChunk,MasterNodeList<T> nodeList,
            Dictionary<Node<T>, List<Route<T>>> nodeRoutesDictionary)
        {
            this.Done = false;
            this.sourceDataChunk = sourceDataChunk;
            this.sourceDataIndex = 0;
            this.nodeList = nodeList;
            this.connectedNodes = new List<Node<T>>()
            {
                nodeList.newStartNode(sourceDataChunk)
            };
            this.nodeRoutesDictionary = nodeRoutesDictionary;
            this.addedLinks = new List<DirectedPair<T>>();
        }

        public void computeFullRoute()
        {
            while (!this.Done)
            {
                appendToRoute();
            }
        }

        public void appendToRoute()
        {
            if (this.Done)
            {
                return;
            }

            var nextValueSequence = SourceData.Skip(sourceDataIndex);
            if (nextValueSequence.Count() > 0)
            {
                ValueNode<T> topCandidateNode = findTopCandidateNode(nextValueSequence);
                if (topCandidateNode == null)
                {
                    topCandidateNode = nodeList.newValueNodeFromValue(nextValueSequence.ElementAt(0));
                }
                    
                proceedToNode(topCandidateNode);
            }

            sourceDataIndex++;
            if (sourceDataIndex >= SourceData.Count())
            {
                EndNode<T> endNode = nodeList.newEndNode(sourceDataChunk);
                appendEdgeTo(endNode);
                this.Done = true;
            }
        }

        private void proceedToNode(ValueNode<T> node)
        {
            if (!suitableEdgeExists(node))
            {
                appendEdgeTo(node);
            }
            else
            {
                connectedNodes.Add(node);
            }
        }

        private bool suitableEdgeExists(ValueNode<T> node)
        {
            Node<T> previousLastNode = connectedNodes[connectedNodes.Count - 1];
            if (previousLastNode is StartNode<T>)
            {
                return false;
            }
            else
            {
                ValueNode<T> previousLastValueNode = previousLastNode as ValueNode<T>;
                RouteCriterion<T> criterion = new RouteCriterion<T>()
                {
                    desiredSequence = new List<T>() { previousLastValueNode.Value, node.Value },
                    previousNodeSequence = this.connectedNodes
                };
                return (previousLastValueNode.OutgoingRoutes.Any(route =>
                    route.prefixMatches(criterion)));
            }
        }

        private ValueNode<T> findTopCandidateNode(IEnumerable<T> desiredSequence)
        {
            T nextValue = desiredSequence.ElementAt(0);
            IEnumerable<ValueNode<T>> matchingUnusedNodes =
                nodeList.getValueNodesByValue(nextValue).Except(connectedNodes.OfType<ValueNode<T>>());
            if (matchingUnusedNodes.Count() == 0)
            {
                return null;
            }
            else if (matchingUnusedNodes.Count() == 1)
            {
                return matchingUnusedNodes.ElementAt(0);
            }
            else
            {
                return lookAheadForBestNode(desiredSequence, matchingUnusedNodes);
            }
        }

        private ValueNode<T> lookAheadForBestNode(IEnumerable<T> desiredSequence, IEnumerable<ValueNode<T>> candidateNodes)
        {
            int depth = 1;
            int numberOfRemainingCandidates = candidateNodes.Count();
            RouteCriterion<T> criterion = new RouteCriterion<T>() 
            { 
                desiredSequence = desiredSequence.Take(depth),
                previousNodeSequence = this.connectedNodes
            };
            IEnumerable<Route<T>> previousRemainingRoutes;
            IEnumerable<Route<T>> remainingRoutes = new List<Route<T>>();
            foreach (ValueNode<T> candidate in candidateNodes)
            {
                previousRemainingRoutes = getInitialNodeRoutes(candidate, criterion);
                remainingRoutes = remainingRoutes.Concat(previousRemainingRoutes);
            }            
            previousRemainingRoutes = remainingRoutes;

            while (remainingRoutes.Count() > 1)
            {
                previousRemainingRoutes = remainingRoutes;
                depth++;
                remainingRoutes = findMatchingRoutes(desiredSequence.Take(depth), remainingRoutes);
            }

            if (remainingRoutes.Count() == 0)
            {
                if (previousRemainingRoutes.Count() == 0)
                {
                    return candidateNodes.ElementAt(0);
                }
                else
                {
                    return previousRemainingRoutes.ElementAt(0).startNode as ValueNode<T>;
                }
            }
            else
            {
                return remainingRoutes.ElementAt(0).startNode as ValueNode<T>;
            }
        }

        private IEnumerable<Route<T>> getInitialNodeRoutes(ValueNode<T> routeStartingNode,RouteCriterion<T> criterion)
        {
            IEnumerable<Route<T>> cachedRoutes = Enumerable.Empty<Route<T>>();
            if (nodeRoutesDictionary.ContainsKey(routeStartingNode))
            {
                cachedRoutes = nodeRoutesDictionary[routeStartingNode];
            }

            IEnumerable<Route<T>> allMatchingEdges = routeStartingNode.findMatchingRoutes(criterion);
            IEnumerable<Route<T>> allNewMatchingEdges = allMatchingEdges.Where(route =>
                route.IsNotStartOfAny(cachedRoutes));
            foreach (var route in allNewMatchingEdges)
            {
                if (!nodeRoutesDictionary.ContainsKey(routeStartingNode))
                {
                    nodeRoutesDictionary[routeStartingNode] = new List<Route<T>>();
                }
                nodeRoutesDictionary[routeStartingNode].Add(route);
            }

            return cachedRoutes.Concat<Route<T>>(allNewMatchingEdges);    
        }

        private IEnumerable<Route<T>> findMatchingRoutes(IEnumerable<T> desiredSequence, IEnumerable<Route<T>> remainingRoutes)
        {
            RouteCriterion<T> criterion = new RouteCriterion<T>()
            {
                desiredSequence = desiredSequence,
                previousNodeSequence = this.connectedNodes
            };
            IEnumerable<Route<T>> routesToCheck;
            foreach (Route<T> route in remainingRoutes)
            {
                routesToCheck = new List<Route<T>>() { route };
                if (route.connectedNodes.Count() < desiredSequence.Count())
                {
                    routesToCheck = computeFartherRoutes(route,desiredSequence.Count());
                }
                foreach (Route<T> longEnoughRoute in routesToCheck)
                {
                    if (longEnoughRoute.prefixMatches(criterion))
                    {
                        yield return longEnoughRoute;
                    }
                }
            }
        }

        private IEnumerable<Route<T>> computeFartherRoutes(Route<T> baseRoute,int desiredDepth)
        {
            int depth = baseRoute.connectedNodes.Count();
            while (depth < desiredDepth)
            {
                depth++;
                foreach (Route<T> route in followOutgoingRoutes(baseRoute))
                {
                    yield return route;
                }
            }
        }

        private IEnumerable<Route<T>> followOutgoingRoutes(Route<T> baseRoute)
        {
            Node<T> firstNode = baseRoute.startNode;
            if (nodeRoutesDictionary.ContainsKey(firstNode))
            {
                nodeRoutesDictionary[firstNode].Remove(baseRoute);
            }
            else
            {
                nodeRoutesDictionary[firstNode] = new List<Route<T>>();
            }

            Node<T> lastNode = baseRoute.connectedNodes.Last();
            RouteFactory<T> routeFactory = new RouteFactory<T>();
            Route<T> newLongerRoute;

            foreach (Route<T> route in lastNode.OutgoingRoutes)
            {
                newLongerRoute = routeFactory.newRouteFromConnectedRoutes(baseRoute, route);
                nodeRoutesDictionary[firstNode].Add(newLongerRoute);
                yield return newLongerRoute;
            }
        }

        private void appendEdgeTo(Node<T> nextNode)
        {
            Node<T> previousLastNode = connectedNodes[connectedNodes.Count - 1];
            Edge<T> newEdge;
            DirectedPair<T> newLink = new DirectedPair<T>()
            {
                from = previousLastNode,
                to = nextNode
            };
            if (previousLastNode.GetType() == typeof(StartNode<T>))
            {
                newEdge =
                    new Edge<T>()
                    {
                        link = newLink
                    };
            }
            else
            {
                DirectedPair<T> lastAddedLink = latestLinkBeforeOtherRouteReqs(previousLastNode);
                newEdge =
                    new Edge<T>()
                    {
                        link = newLink,
                        requisiteLink = lastAddedLink
                    };
            }
            addNewLinkIfDifferent(newLink);            
            Route<T> newRoute = new RouteFactory<T>().newRouteFromEdge(newEdge);
            connectedNodes.Add(nextNode);
        }

        private DirectedPair<T> latestLinkBeforeOtherRouteReqs(Node<T> node)
        {
            int earliestRequisiteIndex = connectedNodes.FindIndex(connectedNode =>
                node.OutgoingRoutes.Any(route => route.edge.requisiteLink.from == connectedNode));
            if (earliestRequisiteIndex == -1)
            {
                return addedLinks[addedLinks.Count - 1];
            }
            else
            {
                int linkPositionInNodes;
                for (int linkIndex = addedLinks.Count - 1; linkIndex >= 0; linkIndex--)
                {
                    linkPositionInNodes = connectedNodes.FindIndex(connectedNode =>
                        connectedNode == addedLinks[linkIndex].from);
                    if (linkPositionInNodes < earliestRequisiteIndex)
                    {
                        return addedLinks[linkIndex];
                    }
                }
                return addedLinks[0];
            }
        }

        private void addNewLinkIfDifferent(DirectedPair<T> newLink)
        {
            Node<T> fromNode = newLink.from;
            if (!fromNode.OutgoingRoutes.Any(oldRoute => oldRoute.edge.link.to == newLink.to))
            {
                addedLinks.Add(newLink);
            }
        }
    }
}
