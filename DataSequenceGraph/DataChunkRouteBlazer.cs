using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRouteBlazer<T>
    {
        public DataChunkRoute<T> chunkRoute { get; set; }
        public IEnumerable<T> SourceData 
        { 
            get
            {
                return this.sourceDataChunk;
            }
        }
        public MasterNodeList<T> nodeList { get; set; }
        public bool Done { get; private set; }

        private int sourceDataIndex;
        private List<Node> addedNodes { get; set; }
        private IEnumerable<T> sourceDataChunk { get; set; }

        public DataChunkRouteBlazer(IEnumerable<T> sourceDataChunk,MasterNodeList<T> nodeList)
        {
            this.Done = false;
            this.sourceDataChunk = sourceDataChunk;
            this.sourceDataIndex = 0;
            this.nodeList = nodeList;
            RouteFactory<T> factory = new RouteFactory<T>();
            this.chunkRoute = factory.newDataChunkRoute(nodeList.newGateNode());
            this.addedNodes = new List<Node>() { chunkRoute.startNode };
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
                bool added = false;
                ValueNode<T> topCandidateNode = findTopCandidateNode(nextValueSequence);
                if (topCandidateNode == null)
                {
                    topCandidateNode = nodeList.newValueNodeFromValue(nextValueSequence.ElementAt(0));
                    added = true;
                }
                    
                proceedToNode(topCandidateNode);
                if (added)
                {
                    addedNodes.Add(topCandidateNode);
                }
            }

            sourceDataIndex++;
            if (sourceDataIndex >= SourceData.Count())
            {
                appendEdgeTo(this.chunkRoute.startNode);
                this.Done = true;
            }
        }

        private void proceedToNode(ValueNode<T> node)
        {
            EdgeRoute bestEdge = findBestEdgeTo(node);
            if (bestEdge == null)
            {
                appendEdgeTo(node);
            }
            else
            {
                this.chunkRoute.appendEdge(bestEdge);
            }
        }

        private EdgeRoute findBestEdgeTo(ValueNode<T> node)
        {
            Node previousLastNode = this.chunkRoute.lastNode;
            if (previousLastNode.kind == NodeKind.GateNode)
            {
                return null;
            }
            else
            {
                ValueNode<T> previousLastValueNode = previousLastNode as ValueNode<T>;
                RouteCriterion<T> criterion = new RouteCriterion<T>()
                {
                    desiredSequence = new List<T>() { previousLastValueNode.Value, node.Value },
                    routeSoFar = this.chunkRoute
                };
                return (previousLastValueNode.OutgoingEdges.FirstOrDefault(route =>
                    route.prefixMatches(criterion)));
            }
        }

        private ValueNode<T> findTopCandidateNode(IEnumerable<T> desiredSequence)
        {
            T nextValue = desiredSequence.ElementAt(0);
            IEnumerable<ValueNode<T>> matchingNodes = nodeList.getValueNodesByValue(nextValue);
            IEnumerable<ValueNode<T>> matchingUnusedNodes = this.chunkRoute.excludeMyNodesFrom(matchingNodes);
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
                routeSoFar = this.chunkRoute
            };
            IEnumerable<Route> previousRemainingRoutes;
            IEnumerable<Route> remainingRoutes = new List<Route>();
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

        private IEnumerable<Route> getInitialNodeRoutes(ValueNode<T> routeStartingNode,RouteCriterion<T> criterion)
        {
            updateRoutesListForNode(routeStartingNode);

            return nodeList.nodeRoutesDictionary[routeStartingNode].Where(route => route.prefixMatches(criterion));
        }

        private void updateRoutesListForNode(ValueNode<T> routeStartingNode)
        {
            IEnumerable<Route> cachedRoutes = Enumerable.Empty<Route>();
            if (nodeList.nodeRoutesDictionary.ContainsKey(routeStartingNode))
            {
                cachedRoutes = nodeList.nodeRoutesDictionary[routeStartingNode];
            }
            else
            {
                nodeList.nodeRoutesDictionary[routeStartingNode] = new List<Route>();
            }

            IEnumerable<Route> allNewOutgoingEdges = routeStartingNode.OutgoingEdges.Where(route =>
                route.IsNotStartOfAny(cachedRoutes));
            foreach (var route in allNewOutgoingEdges)
            {
                nodeList.nodeRoutesDictionary[routeStartingNode].Add(route);
            }
        }

        private IEnumerable<Route> findMatchingRoutes(IEnumerable<T> desiredSequence, IEnumerable<Route> remainingRoutes)
        {
            RouteCriterion<T> criterion = new RouteCriterion<T>()
            {
                desiredSequence = desiredSequence,
                routeSoFar = this.chunkRoute
            };
            IEnumerable<Route> routesToCheck;
            foreach (Route route in remainingRoutes)
            {
                routesToCheck = new List<Route>() { route };
                if (route.connectedNodes.Count() < desiredSequence.Count())
                {
                    routesToCheck = computeFartherRoutes(route,desiredSequence.Count());
                }
                foreach (Route longEnoughRoute in routesToCheck)
                {
                    if (longEnoughRoute.prefixMatches(criterion))
                    {
                        yield return longEnoughRoute;
                    }
                }
            }
        }

        private IEnumerable<Route> computeFartherRoutes(Route baseRoute,int desiredDepth)
        {
            int depth = baseRoute.connectedNodes.Count();
            while (depth < desiredDepth)
            {
                depth++;
                foreach (Route route in followOutgoingRoutes(baseRoute))
                {
                    yield return route;
                }
            }
        }

        private IEnumerable<Route> followOutgoingRoutes(Route baseRoute)
        {
            Node firstNode = baseRoute.startNode;
            if (nodeList.nodeRoutesDictionary.ContainsKey(firstNode))
            {
                nodeList.nodeRoutesDictionary[firstNode].Remove(baseRoute);
            }
            else
            {
                nodeList.nodeRoutesDictionary[firstNode] = new List<Route>();
            }

            Node lastNode = baseRoute.connectedNodes.Last();
            RouteFactory<T> routeFactory = new RouteFactory<T>();
            Route newLongerRoute;

            foreach (Route route in lastNode.OutgoingEdges)
            {
                newLongerRoute = routeFactory.newRouteFromConnectedRoutes(baseRoute, route);
                nodeList.nodeRoutesDictionary[firstNode].Add(newLongerRoute);
                yield return newLongerRoute;
            }
        }

        private void appendEdgeTo(Node nextNode)
        {
            Node previousLastNode = chunkRoute.lastNode;
            Edge newEdge;
            DirectedPair newLink = new DirectedPair()
            {
                from = previousLastNode,
                to = nextNode
            };
            if (previousLastNode.kind == NodeKind.GateNode)
            {
                newEdge =
                    new Edge()
                    {
                        link = newLink
                    };
            }
            else if (nextNode.kind == NodeKind.GateNode)
            {
                newEdge =
                    new Edge()
                    {
                        link = newLink,
                        requisiteNodes = new List<Node>() { chunkRoute.startNode }
                    };
            }
            else
            {
                Node lastAddedNode = latestAddedNodeBeforeOutgoingReqs(previousLastNode);
                newEdge =
                    new Edge()
                    {
                        link = newLink,
                        requisiteNodes = new List<Node>() { lastAddedNode }
                    };
            }
            EdgeRoute newRoute = new RouteFactory<T>().newRouteFromEdge(newEdge);
            this.chunkRoute.appendEdge(newRoute);
        }

        private Node latestAddedNodeBeforeOutgoingReqs(Node node)
        {
            int checkedNodeIndex = chunkRoute.findNode(new List<Node> { node });
            int earliestRequisiteIndex = this.chunkRoute.findEarliestMatchOfRequisites(node.OutgoingEdges).Item1;
            int indexToStartSearch = addedNodes.Count - 1;
            if (addedNodes[indexToStartSearch].SequenceNumber == node.SequenceNumber)
            {
                indexToStartSearch--;
            }
            if (indexToStartSearch < 0)
            {
                indexToStartSearch = 0;
            }
            if (earliestRequisiteIndex == -1)
            {
                return addedNodes[indexToStartSearch];
            }
            else
            {
                int linkPositionInNodes;
                for (int linkIndex = indexToStartSearch; linkIndex >= 0; linkIndex--)
                {
                    linkPositionInNodes = this.chunkRoute.findNode(new List<Node>() { addedNodes[linkIndex] });
                    if (linkPositionInNodes < earliestRequisiteIndex)
                    {
                        return addedNodes[linkIndex];
                    }
                }
                return addedNodes[0];
            }
        }
    }
}
