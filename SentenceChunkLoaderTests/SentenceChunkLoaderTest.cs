using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using DataSequenceGraph.Chunk;

namespace SentenceChunkLoaderTests
{
    [TestFixture]
    public class SentenceChunkLoaderTest
    {
        [Test]
        public void simpleTest()
        {
            StringReader testStr = new StringReader("Sentence one. Sentence' two! Sentence three?");
            List<string> sentenceChunks = SentenceChunkLoader.ToSentenceChunks(testStr);
            Assert.AreEqual(3, sentenceChunks.Count());
            Assert.AreEqual("Sentence one", sentenceChunks.ElementAt(0));
            Assert.AreEqual("Sentence' two", sentenceChunks.ElementAt(1));
            Assert.AreEqual("Sentence three", sentenceChunks.ElementAt(2));
        }

        [Test]
        public void wordsTest()
        {
            List<string> wordValues = SentenceChunkLoader.ToWordValues("a bad, gn'ome is\" lickety-split (here) now");
            Assert.AreEqual(7, wordValues.Count);
            Assert.AreEqual("a", wordValues[0]);
            Assert.AreEqual("bad", wordValues[1]);
            Assert.AreEqual("gn'ome", wordValues[2]);
            Assert.AreEqual("is", wordValues[3]);
            Assert.AreEqual("lickety-split", wordValues[4]);
            Assert.AreEqual("here", wordValues[5]);
            Assert.AreEqual("now", wordValues[6]);
        }
    }
}
