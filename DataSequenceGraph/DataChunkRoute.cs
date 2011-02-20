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

        public override IEnumerable<Node> connectedNodes
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

        public Tuple<IEnumerable<NodeSpec>, IEnumerable<EdgeRouteSpec>> specsForMissingComponents(
            MasterNodeList<T> destinationList)
        {
            followToEnd();
            List<NodeSpec> nodeSpecs = new List<NodeSpec>();
            List<EdgeRouteSpec> edgeSpecs = new List<EdgeRouteSpec>();
            DirectedPair link;
            DirectedPair reqLink;
            bool foundAllNodes;
            nodeSpecs.Add(startNode.ToNodeSpec());
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
            return new Tuple<IEnumerable<NodeSpec>,IEnumerable<EdgeRouteSpec>>(nodeSpecs,edgeSpecs);
        }

        private bool edgeOnNodeAlready(MasterNodeList<T> destinationList,Node node, EdgeRoute edge)
        {
            Node indexedNode = destinationList.nodeByNumber(node.SequenceNumber);
            return indexedNode.OutgoingEdges.Any(nodeEdge => nodeEdge.Equals(edge));
        }

        private bool addNodeSpecIfMissing(MasterNodeList<T> destinationList, Node node, List<NodeSpec> specList)
        {
            Node foundNode = destinationList.nodeByNumber(node.SequenceNumber);
            if (foundNode.kind == NodeKind.NullNode)
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
    }
}
