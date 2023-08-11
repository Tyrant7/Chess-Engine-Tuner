using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    /*
    Search:

        Time coefficient - tentative
        Aspiration window widening size
        Aspiration window size
        RFP margin
        NMP R value
        NMP depth coefficient
        EFP margin
        LMR depth margin
        LMR moves tried margin
        LMR R value
        

    Eval:

        Gamephase
        PieceValues
        PSTs
        Tempo - tentative
    */

    public struct RawParameterGroup
    {
        public RawParameterGroup(ParameterGroup group)
        {
            Parameters = new Dictionary<string, int>(group.Parameters.Count);
            foreach (KeyValuePair<string, ParameterGroup.Parameter> par in group.Parameters)
            {
                Parameters.Add(par.Key, par.Value.Value);
            }
        }

        public Dictionary<string, int> Parameters;
    }

    public partial struct ParameterGroup
    {
        public ParameterGroup()
        {
            Parameters = new Dictionary<string, Parameter>
            {
                { "AWWiden",        new Parameter(65) },
                { "AWSize",         new Parameter(20) },
                { "RFPMargin",      new Parameter(100) },
                { "NMP_R",          new Parameter(3) },
                { "NMPDepthCoef",   new Parameter(5) },
                { "EFPMargin",      new Parameter(120) },
                { "LMR_R",          new Parameter(3) },
                { "LMRDepthMargin", new Parameter(3) },
                { "LMRTriedMargin", new Parameter(8) },
            };
        }

        public Dictionary<string, Parameter> Parameters;
    }
}
