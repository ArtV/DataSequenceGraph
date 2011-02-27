using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DataSequenceGraph
{
    [TestFixture]
    public class DataChunkRouteBlazerTest
    {
        List<string> srcDataList = new List<string>() { "A", "B", "C" };
        public MasterNodeList<string> list;
        private DataChunkRouteBlazer<string> chunkRoute;
        private Dictionary<Node, List<Route>> routePrefixDictionary;

        List<string> srcData2List = new List<string>() { "A", "A", "D" };
        private DataChunkRouteBlazer<string> chunkRoute2;

        List<string> srcData3List = new List<string>() { "A", "A", "E" };
        private DataChunkRouteBlazer<string> chunkRoute3;

        List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
        private DataChunkRouteBlazer<string> chunkRoute4;

        List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
        private DataChunkRouteBlazer<string> chunkRoute5;

        List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
        private DataChunkRouteBlazer<string> chunkRoute6;

        public static void threeThreeChunks(MasterNodeList<string> inList,
            Dictionary<Node, List<Route>> inRoutePrefixDictionary)
        {
            DataChunkRouteBlazerTest test = new DataChunkRouteBlazerTest();
            test.list = inList;
            test.routePrefixDictionary = inRoutePrefixDictionary;
            test.dataSetup13();
            test.setup13append();
        }

        public static void threeSixChunks (MasterNodeList<string> inList, 
            Dictionary<Node, List<Route>> inRoutePrefixDictionary)
        {
            DataChunkRouteBlazerTest test = new DataChunkRouteBlazerTest();
            test.list = inList;
            test.routePrefixDictionary = inRoutePrefixDictionary;
            test.dataSetup46();
            test.chunkRouteAppendThriceOther();
        }        

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            routePrefixDictionary = new Dictionary<Node, List<Route>>();
            dataSetUp();
        }

        public void dataSetUp()
        {
            dataSetup13();
            dataSetup46();
        }

        public void dataSetup13()
        {
            chunkRoute = new DataChunkRouteBlazer<string>(srcDataList, list);
            chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2List, list);
            chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3List, list);
        }

        private void setup13append()
        {
            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        private void chunkRouteAppendThrice()
        {
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
        }

        private void chunkRouteAppendThriceOther()
        {
            chunkRoute4.computeFullRoute();
            chunkRoute5.computeFullRoute();
            chunkRoute6.computeFullRoute();
        }

        public void dataSetup46()
        {
            chunkRoute4 = new DataChunkRouteBlazer<string>(srcData4List, list);
            chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5List, list);
            chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6List, list);
        }

        [Test]
        public void chunkRouteInit()
        {
            GateNode firstNode = chunkRoute.chunkRoute.startNode as GateNode;
            Assert.IsNotNull(firstNode);
        }

        [Test]
        public void createMissingValueNode()
        {
            chunkRoute.appendToRoute();
            Assert.AreEqual(1, chunkRoute.chunkRoute.dataChunk.Count());
            string Anode = chunkRoute.chunkRoute.dataChunk.ElementAt(0);
            Assert.AreEqual("A", Anode);
            Route startToA = chunkRoute.chunkRoute.startNode.OutgoingEdges.ElementAt(0);
            Assert.AreEqual(Anode, ((ValueNode<string>) startToA.connectedNodes.Last()).Value);
            Assert.AreEqual(1, chunkRoute.chunkRoute.startNode.OutgoingEdges.Count());
        }

        [Test]
        public void sameChunkNotReusesValueNode()
        {
            int beforeCount = list.getValueNodesByValue("A").Count();
            chunkRoute2.appendToRoute();
            int afterCount = list.getValueNodesByValue("A").Count();
            chunkRoute2.appendToRoute();
            Assert.AreNotEqual(beforeCount, afterCount);
        }



        [Test]
        public void differentChunkReusesValueNode()
        {
            chunkRouteAppendThrice();
            int beforeCount = list.getValueNodesByValue("A").Count();
            chunkRoute2.appendToRoute();
            int afterCount = list.getValueNodesByValue("A").Count();
            Assert.AreEqual(beforeCount, afterCount);
        }

        [Test]
        public void choosesLongerExistingRoute()
        {
            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            int beforeCount = list.getValueNodesByValue("A").Count();
            chunkRoute3.computeFullRoute();
            int afterCount = list.getValueNodesByValue("A").Count();
            Assert.AreEqual(beforeCount, afterCount);
        }

        [Test]
        public void usesEarlierRequisiteOnNewEdge()
        {
            chunkRouteAppendThriceOther();
            ValueNode<string> Gnode = list.getValueNodesByValue("G").First();
            GateNode gate6 = (GateNode) list.nodeByNumber(5);
            ValueNode<string> Dnode = list.getValueNodesByValue("D").First();
            ValueNode<string> Bnode = list.getValueNodesByValue("B").First();
            ValueNode<string> Cnode = list.getValueNodesByValue("C").First();
            Route DJroute = Dnode.OutgoingEdges.First(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("J"));
            Route DMroute = Dnode.OutgoingEdges.Last(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("M"));
            Route DEroute = Dnode.OutgoingEdges.Last(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("E"));
            Assert.AreEqual(Gnode.SequenceNumber , DJroute.requisiteLinks.First().from.SequenceNumber);
            Assert.AreEqual(gate6.SequenceNumber , DMroute.requisiteLinks.First().from.SequenceNumber);
            Assert.AreEqual(Cnode.SequenceNumber, DEroute.requisiteLinks.First().from.SequenceNumber);
            
            ValueNode<string> Onode = list.getValueNodesByValue("O").First();
            Route OgateRoute = Onode.OutgoingEdges.First();
            Assert.AreEqual(gate6.SequenceNumber, OgateRoute.requisiteLinks.First().from.SequenceNumber);
        }

        [Test]
        public void findMissingRouteComponents()
        {
            setup13append();
            MasterNodeList<string> destinationList = new MasterNodeList<string>();
            Dictionary<Node,List<Route>> destinationDict = new Dictionary<Node, List<Route>>();
            DataChunkRoute<string> ABC = list.nthDataChunkRoute(0);
            Tuple<IList<NodeSpec>, IList<EdgeRouteSpec>> missingComponents = 
                ABC.specsForMissingComponents(destinationList);
            IEnumerable<NodeSpec> missingNodes = missingComponents.Item1;
            IEnumerable<EdgeRouteSpec> missingEdges = missingComponents.Item2;
            NodeSpec missingGate = missingNodes.First(node => node.kind == NodeKind.GateNode);
            Assert.AreEqual(1, missingNodes.Where(node => node.kind == NodeKind.GateNode).Count());
            Assert.AreEqual(0, missingGate.SequenceNumber);
            NodeSpec newValueSpec = missingNodes.First(node => node.kind == NodeKind.ValueNode);
            Assert.AreEqual(6, newValueSpec.SequenceNumber);
            Assert.AreEqual(3, missingNodes.Where(node => node.kind == NodeKind.ValueNode).Count());
            Assert.AreEqual(1, missingEdges.Where(edge => edge.FromNumber == missingGate.SequenceNumber).Count());

            destinationList.reloadNodesThenRoutesFromSpecs(missingNodes, missingEdges);
            ValueNode<string> Anode = destinationList.getValueNodesByValue("A").First();
            Assert.AreEqual("A", Anode.Value);
            Assert.AreEqual(6, Anode.SequenceNumber);

            DataChunkRoute<string> AAE = list.nthDataChunkRoute(2);
            missingComponents = AAE.specsForMissingComponents(destinationList);
            missingNodes = missingComponents.Item1;
            missingEdges = missingComponents.Item2;
            destinationList.reloadNodesThenRoutesFromSpecs(missingNodes, missingEdges);
            Assert.AreEqual(2, destinationList.getValueNodesByValue("A").Count());
            int DnodeIndex = list.getValueNodesByValue("D").First().SequenceNumber;
            Assert.AreEqual(NodeKind.NullNode,destinationList.nodeByNumber(DnodeIndex).kind); 
        }
    }
}
