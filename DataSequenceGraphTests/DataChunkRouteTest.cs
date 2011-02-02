using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.DataChunk;

namespace DataSequenceGraph
{
    [TestFixture]
    class DataChunkRouteTest
    {
        List<string> srcDataList = new List<string>() { "A", "B", "C" };
        StringDataChunk srcData; 
        private MasterNodeList<string> list;
        private DataChunkRoute<string> chunkRoute;
        private Dictionary<Node<string>, List<Route<string>>> routePrefixDictionary;

        List<string> srcData2List = new List<string>() { "A", "A", "D" };
        StringDataChunk srcData2;
        private DataChunkRoute<string> chunkRoute2;

        List<string> srcData3List = new List<string>() { "A", "A", "E" };
        StringDataChunk srcData3;
        private DataChunkRoute<string> chunkRoute3;

        List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
        StringDataChunk srcData4;
        private DataChunkRoute<string> chunkRoute4;

        List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
        StringDataChunk srcData5;
        private DataChunkRoute<string> chunkRoute5;

        List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
        StringDataChunk srcData6;
        private DataChunkRoute<string> chunkRoute6;

        [SetUp]
        public void SetUp()
        {
            list = new MasterNodeList<string>();
            routePrefixDictionary = new Dictionary<Node<string>, List<Route<string>>>();
            srcData = new StringDataChunk(srcDataList);
            chunkRoute = new DataChunkRoute<string>(srcData, list,routePrefixDictionary);
            srcData2 = new StringDataChunk(srcData2List);
            chunkRoute2 = new DataChunkRoute<string>(srcData2, list, routePrefixDictionary);
            srcData3 = new StringDataChunk(srcData3List);
            chunkRoute3 = new DataChunkRoute<string>(srcData3, list, routePrefixDictionary);
            srcData4 = new StringDataChunk(srcData4List);
            chunkRoute4 = new DataChunkRoute<string>(srcData4, list, routePrefixDictionary);
            srcData5 = new StringDataChunk(srcData5List);
            chunkRoute5 = new DataChunkRoute<string>(srcData5, list, routePrefixDictionary);
            srcData6 = new StringDataChunk(srcData6List);
            chunkRoute6 = new DataChunkRoute<string>(srcData6, list, routePrefixDictionary);
        }

        [Test]
        public void chunkRouteInit()
        {
            StartNode<string> firstNode = chunkRoute.InitialNode;
            Assert.IsNotNull(firstNode);
        }

        [Test]
        public void createMissingValueNode()
        {
            chunkRoute.appendToRoute();
            Assert.AreEqual(2, chunkRoute.connectedNodes.Count());
            ValueNode<string> Anode = ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1));
            Assert.AreEqual("A", Anode.Value);
            Route<string> startToA = chunkRoute.InitialNode.OutgoingRoutes.ElementAt(0);
            Assert.AreSame(Anode, startToA.connectedNodes.Last());
            Assert.AreEqual(1, chunkRoute.InitialNode.OutgoingRoutes.Count());
        }

        [Test]
        public void createEndNode()
        {
            chunkRouteAppendThrice();
            Assert.IsTrue(chunkRoute.Done);
            Assert.AreEqual(5, chunkRoute.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute.finishNode);
            Route<string> routeToEnd = chunkRoute.connectedNodes.ElementAt(3).OutgoingRoutes.ElementAt(0);
            Assert.AreSame(chunkRoute.finishNode, routeToEnd.connectedNodes.Last());

            Assert.IsFalse(chunkRoute2.Done);
            chunkRoute2.computeFullRoute();
            Assert.IsTrue(chunkRoute2.Done);
            Assert.AreEqual(5, chunkRoute2.connectedNodes.Count());
            Assert.IsInstanceOf<EndNode<string>>(chunkRoute2.finishNode);
        }

        [Test]
        public void sameChunkNotReusesValueNode()
        {
            chunkRoute2.appendToRoute();
            chunkRoute2.appendToRoute();
            ValueNode<string> AnodeFirst = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(1));
            ValueNode<string> AnodeSecond = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(2));
            Assert.AreNotSame(AnodeSecond, AnodeFirst);
        }

        private void chunkRouteAppendThrice()
        {
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
            chunkRoute.appendToRoute();
        }

        [Test]
        public void differentChunkReusesValueNode()
        {
            chunkRouteAppendThrice();
            chunkRoute2.appendToRoute();
            ValueNode<string> Anode = ((ValueNode<string>)chunkRoute.connectedNodes.ElementAt(1));
            ValueNode<string> otherChunkNode = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(1));
            Assert.AreSame(Anode, otherChunkNode);
        }

        [Test]
        public void choosesLongerExistingRoute()
        {
            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();

            ValueNode<string> AnodeTwo = ((ValueNode<string>)chunkRoute2.connectedNodes.ElementAt(1));
            ValueNode<string> route3ANode = ((ValueNode<string>)chunkRoute3.connectedNodes.ElementAt(1));

            Assert.AreSame(AnodeTwo, route3ANode);
        }

        [Test]
        public void usesEarlierRequisiteOnNewEdge()
        {
            chunkRoute4.computeFullRoute();
            chunkRoute5.computeFullRoute();
            chunkRoute6.computeFullRoute();

            ValueNode<string> Dnode = chunkRoute5.connectedNodes.ElementAt(4) as ValueNode<string>;
            Assert.AreEqual("D", Dnode.Value);
            Route<string> DJroute = Dnode.OutgoingRoutes.First(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("J"));
            Route<string> DMroute = Dnode.OutgoingRoutes.Last(route =>
                ((ValueNode<string>)route.connectedNodes.ElementAt(1)).Value.Equals("M"));
            int DJrouteIndex = chunkRoute6.connectedNodes.FindIndex(node => node == DJroute.requisiteLinks.First().from);
            int DMrouteIndex = chunkRoute6.connectedNodes.FindIndex(node => node == DMroute.requisiteLinks.First().from);
            Assert.Less(DMrouteIndex, DJrouteIndex);
        }
    }
}
