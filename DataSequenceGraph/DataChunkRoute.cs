using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class DataChunkRoute<T> : Route
    {
        public IEnumerable<T> dataChunk
        {
            get
            {
                foreach (ValueNode<T> node in chunkRoute.connectedNodes.OfType<ValueNode<T>>().Distinct())
                {
                    yield return node.Value;
                }
            }
        }

        public IList<EdgeRoute> componentEdges
        {
            get
            {
                return _componentEdges.AsReadOnly();
            }
        }

        private Route chunkRoute { get; set; }
        private RouteFactory<T> routeFactory { get; set; }
        private List<EdgeRoute> _componentEdges { get; set; }

        public override IList<Node> connectedNodes
        {
            get 
            {
                return chunkRoute.connectedNodes;
            }
        }

        public override IEnumerable<DirectedPair> requisiteLinks
        {
            get 
            {
                return chunkRoute.requisiteLinks;
            }
        }

        internal DataChunkRoute(RouteFactory<T> routeFactory, GateNode startNode)
        {
            this.routeFactory = routeFactory;
            this.chunkRoute = routeFactory.newRouteFromNode(startNode);
            this._componentEdges = new List<EdgeRoute>();
        }

        public void appendEdge(EdgeRoute edge)
        {
            chunkRoute = routeFactory.newRouteFromConnectedRoutes(this.chunkRoute, edge);
            _componentEdges.Add(edge);
        }

        public IEnumerable<ValueNode<T>> excludeMyNodesFrom(IEnumerable<ValueNode<T>> otherNodes)
        {
            return otherNodes.Except<ValueNode<T>>(this.chunkRoute.connectedNodes.OfType<ValueNode<T>>());
        }

        public void followToEnd()
        {
            EdgeRoute nextRoute = findNextEdgeToFollow();
            while (nextRoute.edge.link.to != startNode)
            {
                appendEdge(nextRoute);
                nextRoute = findNextEdgeToFollow();
            }
        }

        public void followUntilNode(int sequenceNumber)
        {
            while (lastNode.SequenceNumber != sequenceNumber)
            {
                appendEdge(findNextEdgeToFollow());                
            }
        }

        public Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>> specsForMissingComponents(
            MasterNodeList<T> destinationList)
        {
            followToEnd();
            List<NodeSpec> nodeSpecs = new List<NodeSpec>();
            List<EdgeRouteSpec> edgeSpecs = new List<EdgeRouteSpec>();
            DirectedPair link;
            DirectedPair reqLink;
            bool foundAllNodes;
            addNodeSpecIfMissing(destinationList, startNode, nodeSpecs);
            foreach (EdgeRoute edge in _componentEdges)
            {
                link = edge.edge.link;
                reqLink = edge.edge.requisiteLink;

                foundAllNodes = addNodeSpecIfMissing(destinationList,link.to,nodeSpecs);
                foundAllNodes = foundAllNodes && addNodeSpecIfMissing(destinationList,reqLink.from,nodeSpecs);
                foundAllNodes = foundAllNodes && addNodeSpecIfMissing(destinationList,reqLink.to,nodeSpecs);

                if (!foundAllNodes || !edgeOnNodeAlready(destinationList,link.from,edge))                     
                {
                    edgeSpecs.Add(edge.ToEdgeRouteSpec());
                }
            }
            sortAndDedupe(nodeSpecs);
            return new Tuple<IList<NodeSpec>,IList<EdgeRouteSpec>>(nodeSpecs.AsReadOnly(),edgeSpecs.AsReadOnly());
        }

        public IList<NodeAndReqSpec> comboSpecsForMissingComponents(MasterNodeList<T> destinationList)
        {
            List<NodeAndReqSpec> specList = new List<NodeAndReqSpec>();
            followToEnd();
            DirectedPair link;
            DirectedPair reqLink;
            EdgeRoute curRoute;
            NodeAndReqSpec curSpec;
            bool fromIsMissing;
            bool edgeOrReqIsMissing;
            bool nodeIsNecessaryForPreviousEdge = false;
            bool nextEdgeNodeIsNecessary = false;
            for (int i = 0; i <= _componentEdges.Count - 1; i++)
            {
                nodeIsNecessaryForPreviousEdge = nextEdgeNodeIsNecessary;
                curRoute = _componentEdges[i];
                link = curRoute.edge.link;
                reqLink = curRoute.edge.requisiteLink;

                if (link.from.kind == NodeKind.GateNode && nodeIsMissing(destinationList,link.from))
                {
                    specList.Add(new NodeAndReqSpec()
                    {
                        insertFrom = true,
                        fromNode = link.from.ToNodeSpec(),
                        reqFromRouteIndex = -1
                    });
                    nextEdgeNodeIsNecessary = true;
                    continue;
                }
                curSpec = new NodeAndReqSpec()
                {
                    fromNode = link.from.ToNodeSpec(),
                    insertFrom = false,
                    reqFromRouteIndex = -1
                };
                fromIsMissing = false;
                edgeOrReqIsMissing = false;                

                fromIsMissing = nodeIsMissing(destinationList, link.from);
                if (fromIsMissing)
                {
                    edgeOrReqIsMissing = true;
                    curSpec.insertFrom = true;                    
                }
                else
                {
                    edgeOrReqIsMissing = isMissingEdgeOrReq(destinationList, link.from, link.to, reqLink.from, reqLink.to);                    
                }

                if (edgeOrReqIsMissing)
                {
                    curSpec.reqFromRouteIndex = findNode(reqLink.from);
                    nextEdgeNodeIsNecessary = true;
                }
                else
                {
                    nextEdgeNodeIsNecessary = false;
                }

                if (fromIsMissing || edgeOrReqIsMissing || nodeIsNecessaryForPreviousEdge)
                {
                    specList.Add(curSpec);
                }
            }
            if (nodeIsNecessaryForPreviousEdge)
            {
                Node finalNode = _componentEdges[_componentEdges.Count - 1].edge.link.to;
                specList.Add(new NodeAndReqSpec()
                {
                    insertFrom = nodeIsMissing(destinationList, finalNode),
                    fromNode = finalNode.ToNodeSpec(),
                    reqFromRouteIndex = -1
                });
            }
            // last edge to gate node is implied, no need for a spec!
            return specList.AsReadOnly();
        }

        private bool edgeOnNodeAlready(MasterNodeList<T> destinationList,Node node, EdgeRoute edge)
        {
            Node indexedNode = destinationList.nodeByNumber(node.SequenceNumber);
            return indexedNode.OutgoingEdges.Any(nodeEdge => nodeEdge.Equals(edge));
        }

        private bool addNodeSpecIfMissing(MasterNodeList<T> destinationList, Node node, List<NodeSpec> specList)
        {
            if (nodeIsMissing(destinationList,node))
            {
                specList.Add(node.ToNodeSpec());
                return false;
            }
            else
            {
                return true;
            }
        }

        private void sortAndDedupe(List<NodeSpec> nodeSpecs)
        {
            nodeSpecs.Sort((spec1, spec2) => spec1.SequenceNumber.CompareTo(spec2.SequenceNumber));
            int i = 0;
            int curSeqNumber;
            while (i <= nodeSpecs.Count - 1)
            {
                curSeqNumber = nodeSpecs[i].SequenceNumber;
                while (((i + 1) <= (nodeSpecs.Count - 1)) &&
                       (nodeSpecs[i + 1].SequenceNumber == curSeqNumber))
                {
                    nodeSpecs.RemoveAt(i + 1);
                }
                i++;
            }
        }

        private bool nodeIsMissing(MasterNodeList<T> destinationList, Node node)
        {
            Node foundNode = destinationList.nodeByNumber(node.SequenceNumber);
            return (foundNode.kind == NodeKind.NullNode && node.kind != NodeKind.NullNode);
        }

        private bool isMissingEdgeOrReq(MasterNodeList<T> destinationList, Node fromNode, Node toNode, Node reqFromNode, Node reqToNode)
        {
            Node otherFrom = destinationList.nodeByNumber(fromNode.SequenceNumber);
            if (otherFrom.kind == NodeKind.NullNode)
            {
                return true;
            }
            Node otherTo = destinationList.nodeByNumber(toNode.SequenceNumber);
            if (otherTo.kind == NodeKind.NullNode)
            {
                return true;
            }
            IEnumerable<EdgeRoute> otherEdges = otherFrom.OutgoingEdges.Where(
                otherOut => otherOut.edge.link.to.SequenceNumber == otherTo.SequenceNumber);
            if (otherEdges.Count() == 0)
            {
                return true;
            }
            return !(otherEdges.Any(otherEdge => 
                otherEdge.edge.requisiteLink.from.SequenceNumber == reqFromNode.SequenceNumber
                && otherEdge.edge.requisiteLink.to.SequenceNumber == reqToNode.SequenceNumber)); 
        }

    }
}
