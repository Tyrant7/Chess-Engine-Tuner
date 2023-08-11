using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                { "AWWiden",        65 },
                { "AWSize",         20 },
                { "RFPMargin",     100 },
                { "NMP_R",           3 },
                { "NMPDepthCoef",    5 },
                { "EFPMargin",     120 },
                { "LMR_R",           3 },
                { "LMRDepthMargin",  3 },
                { "LMRTriedMargin",  8 },
            };
        }

        public Dictionary<string, Parameter> Parameters;
    }
}
