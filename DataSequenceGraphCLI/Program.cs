using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSequenceGraph;
using DataSequenceGraph.Format;
using System.Xml;
using System.IO;
using DataSequenceGraph.Chunk;
using CommandLine;
using DataSequenceGraph.Communication;

namespace DataSequenceGraphCLI
{
    class Program
    {
        static void threeThreeRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcDataList = new List<string>() { "A", "B", "C" };
            List<string> srcData2List = new List<string>() { "A", "A", "D" };
            List<string> srcData3List = new List<string>() { "A", "A", "E" };

            DataChunkRouteBlazer<string> chunkRoute = new DataChunkRouteBlazer<string>(srcDataList, masterNodeList);
            DataChunkRouteBlazer<string> chunkRoute2 = new DataChunkRouteBlazer<string>(srcData2List, masterNodeList);
            DataChunkRouteBlazer<string> chunkRoute3 = new DataChunkRouteBlazer<string>(srcData3List, masterNodeList);

            chunkRoute.computeFullRoute();
            chunkRoute2.computeFullRoute();
            chunkRoute3.computeFullRoute();
        }

        static void threeSixRoutes(MasterNodeList<string> masterNodeList, Dictionary<Node, List<Route>> routePrefixDictionary)
        {
            List<string> srcData4List = new List<string>() { "A", "B", "C", "D", "E", "F" };
            DataChunkRouteBlazer<string> chunkRoute4 = new DataChunkRouteBlazer<string>(srcData4List, masterNodeList);
            chunkRoute4.computeFullRoute();

            List<string> srcData5List = new List<string>() { "G", "B", "C", "D", "J", "K" };
            DataChunkRouteBlazer<string> chunkRoute5 = new DataChunkRouteBlazer<string>(srcData5List, masterNodeList);
            chunkRoute5.computeFullRoute();

            List<string> srcData6List = new List<string>() { "G", "B", "D", "M", "N", "O" };
            DataChunkRouteBlazer<string> chunkRoute6 = new DataChunkRouteBlazer<string>(srcData6List, masterNodeList);
            chunkRoute6.computeFullRoute();
        }

        static void Main(string[] args)
        {
            Args arguments = new Args();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (parser.ParseArguments(args, arguments))
            {
                if (arguments.HandCodedList != -1 && (arguments.InDatFile != null || arguments.InTxtFile != null
                    || arguments.InXMLFile != null))
                {
                    Console.Out.WriteLine("Hand-coded graph is in place of loading a primary graph.");
                }
                else if (arguments.HandCodedList == -1 && arguments.InXMLFile == null && arguments.InDatFile == null
                    && arguments.InSrcFile == null)
                {
                    Console.Out.WriteLine("Must specify a primary graph, and/or a sentence source file to split.");
                }
                else if (arguments.Quiet && arguments.OutXMLFile == null && arguments.OutDatFile == null &&
                    arguments.OutTxtFile == null && arguments.OutChunkFile == null)
                {
                    Console.Out.WriteLine("Quiet display and nowhere to send file output. Nothing to do.");
                }
                else if (arguments.InXMLFile != null && (arguments.InDatFile != null || arguments.InTxtFile != null))
                {
                    Console.Out.WriteLine("For primary graph, specify XML file, or both edge data file and value text file.");
                }
                else if ((arguments.InDatFile == null && arguments.InTxtFile != null) ||
                         (arguments.InDatFile != null && arguments.InTxtFile == null))
                {
                    Console.Out.WriteLine("Primary graph edge data file must be paired with a values text file.");
                }
                else if (arguments.OutXMLFile != null && (arguments.OutDatFile != null || arguments.OutTxtFile != null))
                {
                    Console.Out.WriteLine("For output file, specify XML filename, or both edge data filename and value text filename.");
                }
                else if ((arguments.OutDatFile == null && arguments.OutTxtFile != null) ||
                         (arguments.OutDatFile != null && arguments.OutDatFile == null))
                {
                    Console.Out.WriteLine("Output edge data file must be paired with a values text file.");
                }
                else if (arguments.InSrcFile != null && (arguments.InXMLFile2 != null || arguments.InDatFile2 != null
                    || arguments.InTxtFile2 != null))
                {
                    Console.Out.WriteLine("Sentence source file cannot be processed simultaneously with a secondary graph.");
                }
                else if (arguments.InXMLFile2 != null && (arguments.InDatFile2 != null || arguments.InTxtFile2 != null))
                {
                    Console.Out.WriteLine("For secondary graph, specify XML filename, or both edge data filename and value text filename.");
                }
                else if ((arguments.InDatFile2 == null && arguments.InTxtFile2 != null) ||
                         (arguments.InDatFile2 != null && arguments.InTxtFile2 == null))
                {
                    Console.Out.WriteLine("Secondary graph data file must be paired with a values text file.");
                }
                else
                {
                    if (arguments.OutDatFile == null && arguments.OutTxtFile == null && 
                        arguments.OutXMLFile == null && arguments.OutChunkFile == null)
                    {
                        arguments.Verbose = true;
                    }

                    // ---- stage 1
                    MasterNodeList<string> firstList;
                    MasterNodeList<string> secondList = null;
                    bool dupeList = false;
                    if (arguments.InSrcFile != null && arguments.Missing)
                    {
                        dupeList = true;
                    }
                    if (arguments.InXMLFile != null)
                    {
                        XMLGraphFormat<string> inXml = new XMLGraphFormat<string>(arguments.InXMLFile);
                        inXml.nodeValueParser = new StringNodeValueParser();
                        firstList = inXml.ToNodeListFromFile();
                        if (dupeList)
                        {
                            secondList = inXml.ToNodeListFromFile();
                        }
                    }
                    else if (arguments.InDatFile != null && arguments.InTxtFile != null)
                    {
                        BinaryAndTXTFormat<string> inOth = new BinaryAndTXTFormat<string>(arguments.InDatFile, arguments.InTxtFile);
                        inOth.nodeValueParser = new StringNodeValueParser();
                        firstList = inOth.ToNodeListFromFiles();
                        if (dupeList)
                        {
                            secondList = inOth.ToNodeListFromFiles();
                        }
                    }
                    else if (arguments.HandCodedList == 0)
                    {
                        firstList = setupNodeList33();
                        if (dupeList)
                        {
                            secondList = setupNodeList33();
                        }
                    }
                    else if (arguments.HandCodedList == 1)
                    {
                        firstList = setupNodeList();
                        if (dupeList)
                        {
                            secondList = setupNodeList();
                        }
                    }
                    else
                    {
                        firstList = new MasterNodeList<string>();
                        if (dupeList)
                        {
                            secondList = new MasterNodeList<string>();
                        }
                    }

                    // ---- stage 2
                    IList<NodeAndReqSpec> nodeReqSpecs = null;
                    IList<NodeSpec> nodeSpecs = null;
                    IList<EdgeRouteSpec> edgeSpecs = null;
                    MasterNodeList<string> commonBaseList = null;
                    if (arguments.BaseXMLFile != null ||
                        (arguments.BaseDatFile != null && arguments.BaseTxtFile != null))
                    {
                        if (arguments.BaseXMLFile != null)
                        {
                            XMLGraphFormat<string> inXml3 = new XMLGraphFormat<string>(arguments.BaseXMLFile);
                            inXml3.nodeValueParser = new StringNodeValueParser();
                            commonBaseList = inXml3.ToNodeListFromFile();
                            secondList = inXml3.ToNodeListFromFile();
                        }
                        else if (arguments.BaseDatFile != null && arguments.BaseTxtFile != null)
                        {
                            BinaryAndTXTFormat<string> inOth3 = new BinaryAndTXTFormat<string>(arguments.BaseDatFile, arguments.BaseTxtFile);
                            inOth3.nodeValueParser = new StringNodeValueParser();
                            commonBaseList = inOth3.ToNodeListFromFiles();
                            secondList = inOth3.ToNodeListFromFiles();
                        }

                    } 
                    if (arguments.InXMLFile2 != null)
                    {
                        XMLGraphFormat<string> inXml2 = new XMLGraphFormat<string>(arguments.InXMLFile2);
                        inXml2.nodeValueParser = new StringNodeValueParser();
                        MasterNodeList<string> tempList;
                        if (commonBaseList != null && !arguments.ReapplyLater)
                        {
                            tempList = inXml2.ToNodeListFromFile();
                            secondList.reloadNodesThenRoutesFromSpecs(tempList.AllNodeSpecs, tempList.AllEdgeSpecs);
                            firstList.addLaterChunksThanBaseToOtherList(commonBaseList, secondList);
                            firstList = secondList;
                        }
                        else if (arguments.Chunk == -1 && !arguments.Missing && arguments.LeastChunk == -1)
                        {
                            tempList = inXml2.ToNodeListFromFile();
                            secondList.reloadNodesThenRoutesFromSpecs(tempList.AllNodeSpecs, tempList.AllEdgeSpecs);
                        }
                        else
                        {
                            secondList = inXml2.ToNodeListFromFile();
                        }
                    }
                    else if (arguments.InDatFile2 != null && arguments.InTxtFile2 != null)
                    {
                        BinaryAndTXTFormat<string> inOth2 = new BinaryAndTXTFormat<string>(arguments.InDatFile2, arguments.InTxtFile2);
                        inOth2.nodeValueParser = new StringNodeValueParser();
                        if (commonBaseList != null && !arguments.ReapplyLater)
                        {
                            inOth2.ToNodeListFromFiles(secondList);
                            firstList.addLaterChunksThanBaseToOtherList(commonBaseList, secondList);
                            firstList = secondList;
                        }
                        else if (arguments.Chunk == -1 && !arguments.Missing && arguments.LeastChunk == -1 &&
                            !arguments.ReapplyLater)
                        {
                            inOth2.ToNodeListFromFiles(firstList);   // means to merge 2nd into 1st
                        }
                        else
                        {
                            secondList = inOth2.ToNodeListFromFiles();
                        }
                    }

                    if (arguments.InSrcFile != null)
                    {
                        loadFile(arguments.InSrcFile, firstList, arguments.Quiet);
                    }
                    if (secondList != null && commonBaseList == null)
                    {
                        if (arguments.Chunk != -1)
                        {
                            DataChunkRoute<string> nthRoute = firstList.nthDataChunkRoute(arguments.Chunk - 1);
                            if (arguments.OutDatFile != null || arguments.OutXMLFile == null)
                            {
                                nodeReqSpecs = nthRoute.comboSpecsForMissingComponents(secondList);
                            }
                            if (arguments.OutXMLFile != null)
                            {
                                var missing = nthRoute.specsForMissingComponents(secondList);
                                nodeSpecs = missing.Item1;
                                edgeSpecs = missing.Item2;
                            }
                        }
                        else if (arguments.Missing)
                        {
                            if (arguments.OutDatFile != null || arguments.OutXMLFile == null)
                            {
                                nodeReqSpecs = firstList.getSpecsAbsentIn(secondList);
                            }
                            if (arguments.OutXMLFile != null)
                            {
                                var missing = firstList.getSegregatedSpecsAbsentIn(secondList);
                                nodeSpecs = missing.Item1;
                                edgeSpecs = missing.Item2;
                            }
                        }
                    }
                    else if (arguments.Chunk != -1)
                    {
                        secondList = new MasterNodeList<string>();
                        DataChunkRoute<string> nthRoute = firstList.nthDataChunkRoute(arguments.Chunk - 1);
                        if (arguments.OutXMLFile != null)
                        {
                            var missing = nthRoute.specsForMissingComponents(secondList);
                            nodeSpecs = missing.Item1;
                            edgeSpecs = missing.Item2;
                        }
                        else if (arguments.OutDatFile != null && arguments.OutTxtFile != null)
                        {
                            nodeReqSpecs = nthRoute.comboSpecsForMissingComponents(secondList);
                        }
                        secondList = new MasterNodeList<string>();
                    }
                    else if (arguments.LeastChunk != -1)
                    {
                        if (arguments.OutDatFile != null || arguments.OutXMLFile == null)
                        {
                            nodeReqSpecs = firstList.getRoutePartsForChunksStarting(arguments.LeastChunk - 1);
                        }
                        if (arguments.OutXMLFile != null)
                        {
                            var missing = firstList.getSegregatedRoutePartsForChunksStarting(arguments.LeastChunk - 1);
                            nodeSpecs = missing.Item1;
                            edgeSpecs = missing.Item2;
                        }
                        secondList = new MasterNodeList<string>();
                    }
                    else if (arguments.ReapplyLater && commonBaseList != null)
                    {
                        secondList.addLaterChunksThanBaseToOtherList(commonBaseList, firstList);
                    }
                    else if (arguments.DeltaDirectory != null)
                    {
                        NodeValueParser<string> nodeValueParser = new StringNodeValueParser();
                        NodeValueExporter<string> nodeValueExporter = new ToStringNodeValueExporter<string>();
                        DeltaDirectory deltaDir = new DeltaDirectory(arguments.DeltaDirectory);
                        DeltaRequestResultKind requestResultKind;
                        DeltaResponseResultKind responseResultKind;
                        MasterNodeList<string> newLocalNodeList;
                        if (arguments.MakeDeltaRequest)
                        {
                            DeltaRequest newReq = new DeltaRequest(deltaDir);
                            string newFilename = deltaDir.CurrentBase + ".base";
                            using (TextWriter textWriter = new StreamWriter(new FileStream(newFilename, FileMode.Create)))
                            {
                                newReq.writeDefaultRequest(textWriter);
                            }
                            Console.Out.WriteLine("Wrote new delta request to " + newFilename);
                        }
                        else if (arguments.DeltaRequest != null)
                        {
                            using (TextReader textReader = new StreamReader(new FileStream(arguments.DeltaRequest, FileMode.Open, FileAccess.Read)))
                            {
                                MemoryStream outReq = new MemoryStream();
                                requestResultKind = DeltaRequestHandler.handleDeltaRequest(deltaDir,
                                    deltaDir.getLastFullGraph(nodeValueParser), firstList,
                                    nodeValueExporter, textReader, outReq);
                                if (requestResultKind == DeltaRequestResultKind.Deltas)
                                {
                                    string newFilename = DateTime.Now.ToString("yyyy-MM-dd'T'HH-mm-ss'Z'") + ".tar.gz";
                                    using (FileStream fileStream = new FileStream(newFilename, FileMode.Create))
                                    {
                                        new MemoryStream(outReq.ToArray()).CopyTo(fileStream);
                                    }
                                    Console.Out.WriteLine("Wrote archive response to " + newFilename);
                                }
                                else
                                {
                                    StreamReader resRdr = new StreamReader(new MemoryStream(outReq.ToArray()));
                                    string[] deltArr = DeltaList.readList(resRdr);
                                    using (FileStream fileStream = new FileStream(deltArr[0] + ".base", FileMode.Create))
                                    {
                                        new MemoryStream(outReq.ToArray()).CopyTo(fileStream);
                                    }
                                    Console.Out.WriteLine("Wrote .base counter-request response to " + deltArr[0] + ".base");
                                } 
                            }
                        }
                        else if (arguments.BaseResponseToRequest != null)
                        {
                            using (TextReader textReader = new StreamReader(new FileStream(arguments.BaseResponseToRequest, FileMode.Open, FileAccess.Read)))
                            {
                                MemoryStream outReq = new MemoryStream();
                                responseResultKind = DeltaResponseHandler.handleDeltaBaseResponse(deltaDir, textReader, new StreamWriter(outReq));
                                if (responseResultKind == DeltaResponseResultKind.UpdateRequest)
                                {
                                    StreamReader resRdr = new StreamReader(new MemoryStream(outReq.ToArray()));
                                    string[] deltArr = DeltaList.readList(resRdr);
                                    using (FileStream fileStream = new FileStream(deltArr[0] + ".base", FileMode.Create))
                                    {
                                        new MemoryStream(outReq.ToArray()).CopyTo(fileStream);
                                    }
                                    Console.Out.WriteLine("Wrote new delta request to " + deltArr[0] + ".base");
                                }
                                else
                                {
                                    Console.Out.WriteLine("Matching .base response indicates no new deltas. No action taken.");
                                }
                            }
                        }
                        else if (arguments.ArchiveResponseToRequest != null)
                        {
                            using (FileStream fs = new FileStream(arguments.ArchiveResponseToRequest, FileMode.Open, FileAccess.Read))
                            {
                                responseResultKind = DeltaResponseHandler.handleDeltaArchiveResponse(firstList, deltaDir,
                                     nodeValueParser, nodeValueExporter, fs, out newLocalNodeList);
                                if (responseResultKind == DeltaResponseResultKind.Acceptance ||
                                    responseResultKind == DeltaResponseResultKind.Rewrite)
                                {
                                    Console.Out.WriteLine("Delta archive has been loaded.");
                                    firstList = newLocalNodeList;
                                }
                                else
                                {
                                    Console.Out.WriteLine("Delta archive has been ignored.");
                                }
                            }
                        }
                    }


                    // ---- stage 3                    
                    if (arguments.Verbose && arguments.Chunk == -1 && arguments.LeastChunk == -1)
                    {
                        defaultTestOutput(firstList);
                        printEnumeratedChunks(firstList);
                    }
                    if (arguments.Chunk != -1 && !arguments.Quiet)
                    {
                        printChunk(firstList.nthDataChunkRoute(arguments.Chunk - 1));
                    }
                    if (arguments.LeastChunk != -1 && !arguments.Quiet)
                    {
                        foreach (var chRoute in firstList.chunkRoutesStartingAt(arguments.LeastChunk - 1))
                        {
                            printChunk(chRoute);
                        }
                    }
                    if ((arguments.Chunk != -1 || arguments.LeastChunk != -1) && 
                        arguments.Verbose && nodeReqSpecs != null)
                    {
                        nodeAndReqOutput(nodeReqSpecs);
                    }
                    if (arguments.Missing && !arguments.Quiet && nodeReqSpecs != null)
                    {
                        nodeAndReqOutput(nodeReqSpecs);
                    }

                    // ---- stage 4
                    if (arguments.OutXMLFile != null)
                    {
                        XMLGraphFormat<string> outX = new XMLGraphFormat<string>(arguments.OutXMLFile);
                        if (nodeSpecs != null)
                        {
                            outX.ToXMLFile(secondList, nodeSpecs, edgeSpecs);
                        }
                        else
                        {
                            outX.ToXMLFile(firstList);
                        }
                    }

                    if (arguments.OutDatFile != null && arguments.OutTxtFile != null)
                    {
                        BinaryAndTXTFormat<string> outOth = new BinaryAndTXTFormat<string>(arguments.OutDatFile, arguments.OutTxtFile);
                        if (nodeReqSpecs != null)
                        {
                            outOth.ToBinaryAndTXTFiles(firstList, secondList, nodeReqSpecs);
                        }
                        else
                        {
                            outOth.ToBinaryAndTXTFiles(firstList);
                        }
                    }

                    if (arguments.OutChunkFile != null)
                    {
                        IEnumerable<IEnumerable<string>> dumpChunks = Enumerable.Empty<IEnumerable<string>>();
                        if (arguments.Chunk != -1)
                        {
                            DataChunkRoute<string> dumpedChunk = firstList.nthDataChunkRoute(arguments.Chunk - 1);
                            dumpChunks = new List<IEnumerable<string>>() { dumpedChunk.dataChunk };
                        }
                        else if (arguments.LeastChunk != -1)
                        {
                            dumpChunks = firstList.chunkRoutesStartingAt(arguments.LeastChunk - 1)
                                .Select(chRoute => chRoute.dataChunk);
                        }
                        else if (arguments.Missing)
                        {
                            dumpChunks = firstList.dataChunksLaterThan(secondList);
                        }
                        using (TextWriter textWriter = new StreamWriter(new FileStream(arguments.OutChunkFile, FileMode.Create)))
                        {
                            foreach (var dumpChunk in dumpChunks)
                            {
                                foreach (var dumpVal in dumpChunk)
                                {
                                    textWriter.Write(dumpVal + " ");
                                }
                                textWriter.WriteLine(".");
                            }
                        }
                    }


                }
            }
        }

        static void loadFile(string filename,MasterNodeList<string> masterNodeList,bool quiet)
        {
            List<string> sentences;
            List<string> words;
            DataChunkRouteBlazer<string> blazer;
            using (TextReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                sentences = SentenceChunkLoader.ToSentenceChunks(reader);
            }
            foreach (string sentence in sentences)
            {
                words = SentenceChunkLoader.ToWordValues(sentence);
                blazer = new DataChunkRouteBlazer<string>(words, masterNodeList);
                blazer.computeFullRoute();
                if (!quiet)
                {
                    Console.Out.WriteLine("Added sentence starting with " + words.First() + " at start node " +
                        blazer.chunkRoute.startNode.SequenceNumber);
                }
            }
        }

        static void nodeAndReqOutput(IEnumerable<NodeAndReqSpec> specs)
        {
            Console.Out.WriteLine();
            foreach(NodeAndReqSpec spec in specs)
            {
                NodeSpec node = spec.fromNode;
                Console.Out.Write(node.SequenceNumber);
                if (node.kind == NodeKind.GateNode)
                {
                    Console.Out.Write(" Gate");
                }
                else
                {
                    Console.Out.Write(" Value \"" + (spec.fromNode as ValueNodeSpec<string>).Value +
                        "\"");
                }
                Console.Out.Write(spec.insertFrom ? " (new)" : "");
                if (spec.fromNode.kind == NodeKind.GateNode)
                {
                    Console.Out.WriteLine(" no requisite to next required");
                }
                else if (spec.usePrevEdgeAsReq)
                {
                    Console.Out.WriteLine(" requisite to next is prev. edge");
                }
                else if (spec.useStartEdgeAsReq)
                {
                    Console.Out.WriteLine(" requisite to next is starting edge");
                }
                else if (spec.reqFromRouteIndex == -1)
                {
                    Console.Out.WriteLine(" no implied edge to next node");
                }
                else
                {
                    Console.Out.WriteLine(" requisite is route edge #" + spec.reqFromRouteIndex); 
                }
            } 
        }

        static void defaultTestOutput(MasterNodeList<string> masterNodeList)
        {
            Console.Out.WriteLine();
            foreach (var node in masterNodeList.AllNodes)
            {
                if (node.kind == NodeKind.NullNode)
                {
                    continue;
                }
                string typ = (node.kind == NodeKind.GateNode ? "Gate" : "Value:");
                string outStr = node.SequenceNumber + " " +  typ + " ";
                if (node.kind == NodeKind.ValueNode)
                {
                    outStr += (node as ValueNode<string>).Value;
                }
                Console.Out.WriteLine(outStr);
                foreach (EdgeRoute route in node.OutgoingEdges)
                {
                    outStr = "   .." +
                        route.connectedNodes.ElementAt(1).SequenceNumber +
                        " if already " + route.edge.requisiteLink.from.SequenceNumber + ".." +
                        route.edge.requisiteLink.to.SequenceNumber;
                    Console.Out.WriteLine(outStr);
                }
            }

        }

        static void printEnumeratedChunks(MasterNodeList<string> masterNodeList)
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine("------ Chunks in graph: ");
            var ind = 1;
            foreach (DataChunkRoute<string> route in masterNodeList.enumerateDataChunkRoutes())
            {
                Console.Out.Write("#" + ind + ": ");
                printChunk(route);
                ind++;
            }
        }

        static void printChunk(DataChunkRoute<string> route)
        {
            Console.Out.WriteLine();
            ValueNode<string> lastNode = null;
            foreach (EdgeRoute edge in route.componentEdges)
            {
                Console.Out.Write(edge.edge.link.from.SequenceNumber);
                if (edge.edge.link.from is ValueNode<string>)
                {
                    Console.Out.Write(":" + ((ValueNode<string>)edge.edge.link.from).Value);
                }
                Console.Out.Write("..");
                lastNode = edge.edge.link.to as ValueNode<string>;
            }
            Console.Out.Write(lastNode.SequenceNumber + ":" + lastNode.Value);
            Console.Out.WriteLine();
        }

        static MasterNodeList<string> setupNodeList()
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> routePrefixDictionary = new Dictionary<Node, List<Route>>();

            threeSixRoutes(masterNodeList, routePrefixDictionary);
            return masterNodeList;
        }

        static MasterNodeList<string> setupNodeList33()
        {
            MasterNodeList<string> masterNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> routePrefixDictionary = new Dictionary<Node, List<Route>>();

            threeThreeRoutes(masterNodeList, routePrefixDictionary);
            return masterNodeList;
        }
    }
}
