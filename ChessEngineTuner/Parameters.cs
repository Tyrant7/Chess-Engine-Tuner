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

    /// <summary>
    /// Define your parameters here.
    /// </summary>
    public partial struct ParameterGroup
    {
        public ParameterGroup()
        {
            Parameters = new Dictionary<string, Parameter>
            {
                { "AWWiden",        new Parameter(65) },
                { "AWSize",         new Parameter(20) },
                { "RFPDepthMargin", new Parameter(8) },
                { "RFPMargin",      new Parameter(100) },
                { "NMP_R",          new Parameter(3) },
                { "NMPDepthCoef",   new Parameter(5) },
                { "EFPDepthMargin", new Parameter(8) },
                { "EFPMargin",      new Parameter(120) },
                { "LMR_R",          new Parameter(3) },
                { "LMRDepthMargin", new Parameter(3) },
                { "LMRTriedMargin", new Parameter(8) },
            };
        }

        public Dictionary<string, Parameter> Parameters;
    }
}
