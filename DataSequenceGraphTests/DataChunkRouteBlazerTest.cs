using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.DataChunk;

namespace DataSequenceGraph
{
    [TestFixture]
    public class DataChunkRouteBlazerTest
    {
        List<string> srcDataList = new List<string>() { "A", "B", "C" };
        StringDataChunk srcData; 
        public MasterNodeList<string> list;
        private DataChunkRouteBlazer<string> chunkRoute;
        private Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary;

        List<string> srcData2List = new List<string>() { "A", "A", "D" };
        StringDataChunk srcData2;
        private DataChunkRouteBlazer<string> chunkRoute2;

        List<string> srcData3List = new List<string>() { "A", "A", "E" };
        StringDataChunk srcData3;
        private DataChunkRouteBlazer<string> chunkRoute3;

        List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
        StringDataChunk srcData4;
        private DataChunkRouteBlazer<string> chunkRoute4;

        List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
        StringDataChunk srcData5;
        private DataChunkRouteBlazer<string> chunkRoute5;

        List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
        StringDataChunk srcData6;
        private DataChunkRouteBlazer<string> chunkRoute6;

        public static void threeSixChunks (MasterNodeList<string> inList, 
            Dictionary<Node<string>, List<Route<string>>> inRoutePrefixDictionary)
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
            routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();
            dataSetUp();
        }

        public void dataSetUp()
        {
            srcData = new StringDataChunk(srcDataList);
            chunkRoute = new DataChunkRouteBlazer<string>(srcData, list, routePrefixDictionary);
            srcData2 = new StringDataChunk(srcData2List);
            chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2, list, routePrefixDictionary);
            srcData3 = new StringDataChunk(srcData3List);
            chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3, list, routePrefixDictionary);
            dataSetup46();
        }

        public void dataSetup46()
        {
            srcData4 = new StringDataChunk(srcData4List);
            chunkRoute4 = new DataChunkRouteBlazer<string>(srcData4, list, routePrefixDictionary);
            srcData5 = new StringDataChunk(srcData5List);
            chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5, list, routePrefixDictionary);
            srcData6 = new StringDataChunk(srcData6List);
            chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6, list, routePrefixDictionary);
        }

        [Test]
        public void chunkRouteInit()
        {
            StartNode<string> firstNode = chunkRoute.chunkRoute.getFirstNode();
            Assert.IsNotNull(firstNode);
        }

        [Test]
        public void createMissingValueNode()
        {
            chunkRoute.appendToRoute();
            Assert.AreEqual(1, chunkRoute.chunkRoute.dataChunk.Count());
            string Anode = chunkRoute.chunkRoute.dataChunk.ElementAt(0);
            Assert.AreEqual("A", Anode);
            Route<string> startToA = chunkRoute.chunkRoute.getFirstNode().OutgoingRoutes.ElementAt(0);
            Assert.AreEqual(Anode, ((ValueNode<string>) startToA.connectedNodes.Last()).Value);
            Assert.AreEqual(1, chunkRoute.chunkRoute.getFirstNode().OutgoingRoutes.Count());
        }

        [Test]
        public void createEndNode()
        {
            chunkRouteAppendThrice();
            Assert.IsTrue(chunkRoute.Done);
            Assert.AreEqual(3, chunkRoute.chunkRoute.dataChunk.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute.chunkRoute.getLastNode());

            Assert.IsFalse(chunkRoute2.Done);
            chunkRoute2.computeFullRoute();
            Assert.IsTrue(chunkRoute2.Done);
            Assert.AreEqual(3, chunkRoute2.chunkRoute.dataChunk.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute2.chunkRoute.getLastNode());
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
            Route<string> DJroute = Dnode.OutgoingRoutes.First(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("J"));
            Route<string> DMroute = Dnode.OutgoingRoutes.Last(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("M"));
            int DJrouteIndex = chunkRoute6.chunkRoute.positionOfContainedNode(DJroute.requisiteLinks.First().from);
            int DMrouteIndex = chunkRoute6.chunkRoute.positionOfContainedNode(DMroute.requisiteLinks.First().from);
            Assert.Less(DMrouteIndex, DJrouteIndex);
        }
    }
}
