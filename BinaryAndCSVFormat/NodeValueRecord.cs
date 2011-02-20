using FileHelpers;
using System;

namespace DataSequenceGraph.Format
{

    [DelimitedRecord(",")]
    public sealed class NodeValueRecord
    {

        public Int32 SeqNum;

        [FieldQuoted(QuoteMode.AlwaysQuoted,MultilineMode.AllowForBoth)]
        public String NodeVal;


    }

}