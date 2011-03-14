using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.Communication;
using System.IO;

namespace DataSequenceGraph.CommunicationTests
{
    [TestFixture]
    public class DeltaDirectoryTest
    {
        public const string CURBASE = "0027-2011-03-13T05-36-00Z";
        public const string MIDSTR = "0025-2011-03-13T05-34-00Z";
        private DeltaDirectory dir;

        [SetUp]
        public void SetUp()
        {
            dir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

        [Test]
        public void testDeltasBefore()
        {
            IEnumerable<string> deltas = dir.getDeltasBeforeOrEqual(MIDSTR,2);
            Assert.AreEqual(-1, deltas.First().CompareTo(MIDSTR));
            Assert.AreEqual(2, deltas.Count());
            Assert.AreNotEqual(true, deltas.Contains(MIDSTR));
            Assert.AreEqual(2, dir.getDeltasBeforeOrEqual(CURBASE,2).Count());
        }

        [Test]
        public void testGetCurrentBase()
        {
            Assert.AreEqual(CURBASE, dir.CurrentBase);
        }

        [Test]
        public void testDeltasAfter()
        {
            IEnumerable<string> deltas = dir.getDeltasAfter(MIDSTR);
            Assert.AreEqual(1, deltas.First().CompareTo(MIDSTR));
            Assert.AreEqual(2, deltas.Count());
            Assert.AreEqual(0, dir.getDeltasAfter(CURBASE).Count());
        }
    }
}
