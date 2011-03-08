using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace DataSequenceGraphCLI
{
    sealed class Args
    {
        [Option("q", "quiet", HelpText = "Display no command output.")]
        public bool Quiet = false;

        [Option("v", "verbose", HelpText = "Display all nodes and edges sent to output files.")]
        public bool Verbose = false;

        [Option("x", "loadxml", HelpText = "XML graph file to load as primary.")]
        public string InXMLFile = null;

        [Option("e", "loadedges", HelpText = "Binary file of graph edges to load as primary (pair with -t).")]
        public string InDatFile = null;

        [Option("t", "loadvalues", HelpText = "Text file of graph data values to load as primary (pair with -e).")]
        public string InTxtFile = null;

        [Option("s", "splittext", HelpText = "Text file to be split into sentence chunks made of word values (clashes with -c, -y, -f, -u).")]
        public string InSrcFile = null;

        [Option("X", "outxml", HelpText = "XML graph file for output.")]
        public string OutXMLFile = null;

        [Option("E", "outedges", HelpText = "Binary file of graph edges for output (pair with -T).")]
        public string OutDatFile = null;

        [Option("T", "outvalues", HelpText = "Text file of graph data values for output (pair with -E).")]
        public string OutTxtFile = null;

        [Option("c", "chunk", HelpText = "Process only the nth stored chunk of the primary graph (clashes with -m, -s).")]
        public int Chunk = -1;

        [Option("C", "chunktext", HelpText = "Output file for the full chunk text (requires -c).")]
        public string OutChunkFile = null;

        [Option("y", "load2xml", HelpText = "XML graph file to load as secondary/smaller.")]
        public string InXMLFile2 = null;

        [Option("f", "load2edges", HelpText = "Binary file of graph edges to load as secondary/smaller (pair with -u).")]
        public string InDatFile2 = null;

        [Option("u", "load2values", HelpText = "Text file of graph data values to load as secondary/smaller (pair with -f).")]
        public string InTxtFile2 = null;

        [Option("m", "missing", HelpText = "Instead of adding and sending the result to output, find the difference or missing parts. (conflicts with -c).")]
        public bool Missing = false;
/* future idea?
        [Option("r", "rebase", HelpText = "Apply the secondary graph after this chunk# and then reapply the primary graph's later chunks afterward.")]
        public int Rebase = -1;
*/       

        [Option("h", "hc", HelpText = "Use the numbered hand/hard-coded graph as the primary.")]
        public int HandCodedList = -1;

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {            
            HelpText txt = new HelpText("DataSequenceGraph CLI");
            txt.AddPreOptionsLine(
"  * Supported graph formats are either 1) XML file, 2) binary edge file plus delimited text file of node value strings.");
            txt.AddPreOptionsLine("  * Omitting output file parameters implies verbose display output instead.");
            txt.AddPreOptionsLine("  * -s adds the new chunks from the source file to the primary graph if specified, else a new graph.");
            txt.AddPreOptionsLine("  * -s and -m sends to output the nodes/edges that need to be added for the chunks from the source file.");
            txt.AddPreOptionsLine("  * Without -c or -m, the secondary graph will be merged into the primary graph.");
            txt.AddPreOptionsLine("  * With -c or -m, output is the nodes/edges from the primary graph absent from the secondary/destination graph.");            
            txt.AddOptions(this);
            return txt;
        }

    }
}
