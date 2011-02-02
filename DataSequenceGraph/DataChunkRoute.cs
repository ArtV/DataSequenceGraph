﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T>
    {
        public IEnumerable<T> SourceData { get; private set; }
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
        private List<Edge<T>> addedEdges { get; set; }

        public DataChunkRoute(IEnumerable<T> sourceData,MasterNodeList<T> nodeList,
            Dictionary<Node<T>, List<Route<T>>> nodeRoutesDictionary)
        {
            this.Done = false;
            this.SourceData = sourceData;
            this.sourceDataIndex = 0;
            this.nodeList = nodeList;
            this.connectedNodes = new List<Node<T>>()
            {
                nodeList.newStartNode()
            };
            this.nodeRoutesDictionary = nodeRoutesDictionary;
            this.addedEdges = new List<Edge<T>>();
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
                EndNode<T> endNode = nodeList.newEndNode();
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
            return (previousLastNode.OutgoingRoutes.Any(route =>
                route.connectedNodes.ElementAt(1) == node));
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
                    return null;
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
            if (previousLastNode.GetType() == typeof(StartNode<T>))
            {
                newEdge =
                    new Edge<T>()
                    {
                        link = new DirectedPair<T>() 
                        { 
                            from = previousLastNode, 
                            to = nextNode 
                        }
                    };
            }
            else
            {
                Edge<T> lastAddedEdge = addedEdges[addedEdges.Count - 1];
                newEdge =
                    new Edge<T>()
                    {
                        link = new DirectedPair<T>()
                        {
                            from = previousLastNode,
                            to = nextNode
                        },
                        requisiteLink = new DirectedPair<T>()
                        {
                            from = lastAddedEdge.link.from,
                            to = lastAddedEdge.link.to
                        }
                    };
            }
            addedEdges.Add(newEdge);
            Route<T> newRoute = new RouteFactory<T>().newRouteFromEdge(newEdge);
            connectedNodes.Add(nextNode);
        }
    }
}
