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
            chunkRoute = new DataChunkRouteBlazer<string>(srcDataList, list, routePrefixDictionary);
            chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2List, list, routePrefixDictionary);
            chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3List, list, routePrefixDictionary);
            dataSetup46();
        }

        public void dataSetup46()
        {
            chunkRoute4 = new DataChunkRouteBlazer<string>(srcData4List, list, routePrefixDictionary);
            chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5List, list, routePrefixDictionary);
            chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6List, list, routePrefixDictionary);
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
            Route startToA = chunkRoute.chunkRoute.startNode.OutgoingRoutes.ElementAt(0);
            Assert.AreEqual(Anode, ((ValueNode<string>) startToA.connectedNodes.Last()).Value);
            Assert.AreEqual(1, chunkRoute.chunkRoute.startNode.OutgoingRoutes.Count());
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

            ValueNode<string> Dnode = list.getValueNodesByValue("D").First();
            Route DJroute = Dnode.OutgoingRoutes.First(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("J"));
            Route DMroute = Dnode.OutgoingRoutes.Last(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("M"));
            int DJrouteIndex = chunkRoute6.chunkRoute.findNode(DJroute.requisiteLinks.First().from);
            int DMrouteIndex = chunkRoute6.chunkRoute.findNode(DMroute.requisiteLinks.First().from);
            Assert.Less(DMrouteIndex, DJrouteIndex);
        }
    }
}
