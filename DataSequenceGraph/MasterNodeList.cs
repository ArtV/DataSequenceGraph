﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class MasterNodeList<T>
    {
        private List<Node> nodeList = new List<Node>();
        private Dictionary<T,List<int>> valueSearchCache = new Dictionary<T,List<int>>();
        private List<GateNode> gateNodeList = new List<GateNode>();
        private RouteFactory<T> routeFactory;

        public Dictionary<Node, List<Route>> nodeRoutesDictionary { get; set; }

        public MasterNodeList()
        {
            routeFactory = new RouteFactory<T>();
            routeFactory.masterNodeList = this;
            nodeRoutesDictionary = new Dictionary<Node, List<Route>>();
        }

        public IEnumerable<Node> AllNodes
        {
            get
            {
                return nodeList;
            }
        }

        public IEnumerable<Node> AllNonNullNodes
        {
            get
            {
                return AllNodes.Where(node => node.kind != NodeKind.NullNode);
            }
        }

        public List<NodeSpec> AllNodeSpecs
        {
            get
            {
                return AllNonNullNodes.Select(node => node.ToNodeSpec()).ToList();
            }
        }

        public IEnumerable<EdgeRouteSpec> AllEdgeSpecs
        {
            get
            {
                return AllNonNullNodes.SelectMany(node => nodeToRoutesSpecs(node));
            }
        }

        private IEnumerable<EdgeRouteSpec> nodeToRoutesSpecs(Node node)
        {
            return node.OutgoingEdges.Select(route => route.ToEdgeRouteSpec());
        }

        public int DataChunkCount
        {
            get
            {
                return gateNodeList.Count;
            }
        }

        public IList<NodeAndReqSpec> AllNodeAndReqSpecs
        {
            get
            {
                return getSpecsAbsentIn(new MasterNodeList<T>());
            }
        }

        public IList<NodeAndReqSpec> getRoutePartsForChunksStarting(int startIndex)
        {
            return getRoutePartsAbsentIn(new MasterNodeList<T>(),
                chunkRoutesStartingAt(startIndex));
        }

        public Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>> getSegregatedRoutePartsForChunksStarting(int startIndex)
        {
            return getSegregatedRoutePartsAbsentIn(new MasterNodeList<T>(),
                chunkRoutesStartingAt(startIndex));
        }

        public IList<NodeAndReqSpec> getSpecsAbsentIn(MasterNodeList<T> otherList)
        {
            return getRoutePartsAbsentIn(otherList, enumerateDataChunkRoutes());
        }

        private IList<NodeAndReqSpec> getRoutePartsAbsentIn(MasterNodeList<T> otherList, IEnumerable<DataChunkRoute<T>> whichRoutes)
        {
            IList<NodeAndReqSpec> specs;
            List<NodeAndReqSpec> overallList = new List<NodeAndReqSpec>();
            foreach (DataChunkRoute<T> route in whichRoutes)
            {
                specs = route.comboSpecsForMissingComponents(otherList);
                overallList.AddRange(specs);
                otherList.reloadNodeAndReqSpecs(specs);
            }
            return overallList.AsReadOnly();
        }

        public Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>> getSegregatedSpecsAbsentIn(MasterNodeList<T> otherList)
        {
            return getSegregatedRoutePartsAbsentIn(otherList, enumerateDataChunkRoutes());
        }

        private Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>> getSegregatedRoutePartsAbsentIn(MasterNodeList<T> otherList, IEnumerable<DataChunkRoute<T>> whichRoutes)
        {
            IList<NodeSpec> nodeSpecs = Enumerable.Empty<NodeSpec>().ToList();
            IList<EdgeRouteSpec> edgeSpecs = Enumerable.Empty<EdgeRouteSpec>().ToList();
            foreach (DataChunkRoute<T> route in whichRoutes)
            {
                var missing = route.specsForMissingComponents(otherList);
                nodeSpecs = nodeSpecs.Concat(missing.Item1).ToList();
                edgeSpecs = edgeSpecs.Concat(missing.Item2).ToList();
                otherList.reloadNodesThenRoutesFromSpecs(nodeSpecs, edgeSpecs);
            }
            return new Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>>(nodeSpecs, edgeSpecs);
        }

        public IEnumerable<ValueNode<T>> getValueNodesByValue(T desiredValue)
        {
            if (!valueSearchCache.ContainsKey(desiredValue))
            {
                yield break;
            }
            else
            {
                foreach (int valueIndex in valueSearchCache[desiredValue])
                {
                    yield return (ValueNode<T>)nodeList[valueIndex];
                }
            }
        }

        public ValueNode<T> newValueNodeFromValue(T newValue)
        {
            ValueNode<T> newNode = new ValueNode<T>(newValue, nodeList.Count);
            AddNode(newNode);
            return newNode;
        }

        private void AddNode(Node newNode)
        {
            nodeList.Add(newNode);
            if (newNode is ValueNode<T>)
            {
                ValueNode<T> newValueNode = newNode as ValueNode<T>;
                cacheValueNode(newValueNode);
            }
            if (newNode is GateNode)
            {
                gateNodeList.Add(newNode as GateNode);
            }
        }

        private void cacheValueNode(ValueNode<T> newValueNode)
        {
            List<int> valueIndexList;
            if (!valueSearchCache.ContainsKey(newValueNode.Value))
            {
                valueIndexList = new List<int>();
                valueSearchCache.Add(newValueNode.Value, valueIndexList);
            }
            else
            {
                valueIndexList = valueSearchCache[newValueNode.Value];
            }
            valueIndexList.Add(newValueNode.SequenceNumber);
        }

        public Node nodeByNumber(int index)
        {
            if (index < 0 || index > nodeList.Count - 1)
            {
                return NullNode.o;
            }
            else
            {
                return nodeList[index];
            }
        }

        public GateNode newGateNode()
        {
            GateNode newNode = new GateNode(nodeList.Count);
            AddNode(newNode);
            return newNode;
        }

        public void addChunksStartingAtIndexToOtherList(int baseIndex, MasterNodeList<T> otherList)
        {
            foreach (var chunk in dataChunksAtOrLaterThanIndex(baseIndex))
            {
                DataChunkRouteBlazer<T> newBlazer = new DataChunkRouteBlazer<T>(chunk, otherList);
                newBlazer.computeFullRoute();
            }
        }

        public void addLaterChunksThanBaseToOtherList(MasterNodeList<T> baseList,MasterNodeList<T> otherList)
        {
            foreach (IEnumerable<T> chunk in dataChunksLaterThan(baseList))
            {
                DataChunkRouteBlazer<T> newBlazer = new DataChunkRouteBlazer<T>(chunk, otherList);
                newBlazer.computeFullRoute();
            }
        }

        public IEnumerable<IEnumerable<T>> dataChunksAtOrLaterThanIndex(int chunkIndex)
        {            
            foreach (var route in chunkRoutesStartingAt(chunkIndex))
            {
                yield return route.dataChunk;
            } 
        }

        public IEnumerable<IEnumerable<T>> dataChunksLaterThan(MasterNodeList<T> pastList)
        {
            return dataChunksAtOrLaterThanIndex(pastList.DataChunkCount);
        }

        public IEnumerable<DataChunkRoute<T>> chunkRoutesStartingAt(int pastCount)
        {
            for (int i = pastCount; i <= gateNodeList.Count - 1; i++)
            {
                DataChunkRoute<T> chunkRoute = nthDataChunkRoute(i);
                chunkRoute.followToEnd();
                yield return chunkRoute;
            }
        }

        public IEnumerable<IEnumerable<T>> enumerateDataChunks()
        {
            foreach (DataChunkRoute<T> route in enumerateDataChunkRoutes())
            {
                yield return route.dataChunk;
            }
        }

        public IEnumerable<DataChunkRoute<T>> enumerateDataChunkRoutes()
        {
            for (int i = 0; i <= gateNodeList.Count - 1; i++)
            {
                yield return nthDataChunkRoute(i);
            }
        }

        public DataChunkRoute<T> nthDataChunkRoute(int nth)
        {
            DataChunkRoute<T> chunkRoute = routeFactory.newDataChunkRoute(gateNodeList[nth]);
            chunkRoute.followToEnd();
            return chunkRoute;
        }       

        public DataChunkRoute<T> dataChunkRouteStartingAt(GateNode node)
        {
            DataChunkRoute<T> chunkRoute = routeFactory.newDataChunkRoute(node);
            chunkRoute.followToEnd();
            return chunkRoute;
        }

        public void reloadNodesFromSpecs(IEnumerable<NodeSpec> specs)
        {
            foreach (NodeSpec spec in specs)
            {
                trySetNode(spec);
            }
        }

        private Node nodeSpecToNode(NodeSpec spec)
        {
            Node returnNode;
            switch (spec.kind)
            {
                case NodeKind.GateNode:
                    returnNode = new GateNode(spec.SequenceNumber);
                    break;
                case NodeKind.ValueNode:
                    returnNode = new ValueNode<T>(((ValueNodeSpec<T>)spec).Value,spec.SequenceNumber);
                    break;
                default:
                    returnNode = NullNode.o;
                    break;
            }
            return returnNode;
        }

        public void reloadNodesThenRoutesFromSpecs(IEnumerable<NodeSpec> nodes, IEnumerable<EdgeRouteSpec> routes)
        {
            reloadNodesFromSpecs(nodes);
            routeFactory.newRoutesFromSpecs(routes);
        }

        public void reloadNodeAndReqSpecs(IList<NodeAndReqSpec> specs)
        {
            EdgeRouteSpec edgeSpec;
            NodeAndReqSpec curSpec;
            NodeAndReqSpec nextSpec;
            DataChunkRoute<T> routeBeingLoaded = null;
            int lastGateNodeIndex = -1;
            for (int i = 0; i <= specs.Count - 1; i++)
            {                
                curSpec = specs[i];
                if (curSpec.fromNode.kind == NodeKind.GateNode)
                {
                    lastGateNodeIndex = curSpec.fromNode.SequenceNumber;
                    trySetNode(curSpec.fromNode);
                    routeBeingLoaded = routeFactory.newDataChunkRoute(gateNodeList[gateNodeList.Count - 1]);
                }
                else
                {
                    if (curSpec.insertFrom)
                    {
                        trySetNode(curSpec.fromNode);
                    }
                    routeBeingLoaded.followUntilNode(curSpec.fromNode.SequenceNumber);
                }
                if ((i + 1) <= specs.Count - 1)
                {
                    nextSpec = specs[i + 1];
                    if (nextSpec.insertFrom)
                    {
                        trySetNode(nextSpec.fromNode);
                    }
                    if (nextSpec.fromNode.kind == NodeKind.GateNode)
                    {
                        edgeSpec = new EdgeRouteSpec()
                        {
                            FromNumber = curSpec.fromNode.SequenceNumber,
                            ToNumber = lastGateNodeIndex,
                            RequisiteToNumber = -1,
                            RequisiteFromNumber = -1
                        };
                        routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
                    }
                    else
                    {
                        bool addEdge = curSpec.fromNode.kind == NodeKind.GateNode;
                        edgeSpec = new EdgeRouteSpec()
                        {
                            FromNumber = curSpec.fromNode.SequenceNumber,
                            ToNumber = nextSpec.fromNode.SequenceNumber,
                            RequisiteFromNumber = -1,
                            RequisiteToNumber = -1
                        };
                        if (curSpec.usePrevEdgeAsReq)
                        {
                            addEdge = true;
                            edgeSpec.RequisiteFromNumber = specs[i - 1].fromNode.SequenceNumber;
                            edgeSpec.RequisiteToNumber = curSpec.fromNode.SequenceNumber;
                        }
                        else if (curSpec.useStartEdgeAsReq)
                        {
                            addEdge = true;
                            edgeSpec.RequisiteFromNumber = routeBeingLoaded.componentEdges[0].edge.link.from.SequenceNumber;
                            edgeSpec.RequisiteToNumber = routeBeingLoaded.componentEdges[0].edge.link.to.SequenceNumber;
                        }
                        else if (curSpec.reqFromRouteIndex != -1)
                        {
                            addEdge = true;
                            EdgeRoute reqEdge = routeBeingLoaded.componentEdges[curSpec.reqFromRouteIndex];
                            edgeSpec.RequisiteFromNumber = reqEdge.edge.link.from.SequenceNumber;
                            edgeSpec.RequisiteToNumber = reqEdge.edge.link.to.SequenceNumber;
                        }
                        if (addEdge)
                        {
                            routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
                        }
                    }
                }                
            }
            if (specs.Count >= 2)
            {
                edgeSpec = new EdgeRouteSpec()
                {
                    FromNumber = specs[specs.Count - 1].fromNode.SequenceNumber,
                    ToNumber = lastGateNodeIndex,
                    RequisiteFromNumber = lastGateNodeIndex,
                    RequisiteToNumber = specs[1].fromNode.SequenceNumber
                };
                routeFactory.newRoutesFromSpecs(new List<EdgeRouteSpec>() { edgeSpec });
            }
        }

        public bool trySetNode(NodeSpec spec)
        {
            int desiredIndex = spec.SequenceNumber;
            if (desiredIndex <= nodeList.Count - 1)
            {
                if (desiredIndex < 0 || nodeList[desiredIndex].kind != NodeKind.NullNode ||
                    spec.kind == NodeKind.NullNode)
                {
                    return false;
                }
                else
                {
                    Node newNode = nodeSpecToNode(spec);
                    nodeList[desiredIndex] = newNode;
                    if (newNode is ValueNode<T>)
                    {
                        ValueNode<T> newValueNode = newNode as ValueNode<T>;
                        cacheValueNode(newValueNode);
                    }
                    if (newNode is GateNode)
                    {
                        gateNodeList.Add(newNode as GateNode);
                    }
                    return true;
                }
            }
            else
            {
                while (desiredIndex > nodeList.Count)
                {
                    nodeList.Add(NullNode.o);
                }
                AddNode(nodeSpecToNode(spec));
                return true;
            }
        }
    }
}
