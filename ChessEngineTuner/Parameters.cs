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
        RFP depth margin
        RFP score margin
        NMP depth margin
        NMP R value
        NMP depth coefficient
        EFP depth margin
        EFP score margin

        LMR depth margin
        LMR tried margin
        LMR base R value
        LMR tried coefficient
        LMR depth coefficient
        

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
                { "AWWiden",        new Parameter(62) },
                { "AWSize",         new Parameter(17) },
                { "RFPDepthMargin", new Parameter(10) },
                { "RFPScoreMargin", new Parameter(96) },
                { "NMPDepthMargin", new Parameter(2) },
                { "NMP_R",          new Parameter(3) },
                { "NMPDepthCoef",   new Parameter(4) },
                { "EFPDepthMargin", new Parameter(8) },
                { "EFPScoreMargin", new Parameter(141) },

                { "LMRTriedMargin", new Parameter(6) },
                { "LMRDepthMargin", new Parameter(2) },
                { "LMR_R",          new Parameter(1) },
                { "LMRTriedCoef",   new Parameter(13) },
                { "LMRDepthCoef",   new Parameter(9) },
            };
        }

        public Dictionary<string, Parameter> Parameters;
    }
}
