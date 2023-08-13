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

    public partial struct ParameterGroup
    {
        public ParameterGroup()
        {
            Parameters = new Dictionary<string, Parameter>
            {
                { "Timer Divisor",          new Parameter(  40,     10,     1,      200)    },
                { "RFP Depth Margin",       new Parameter(  85,     50,     0,      1000)   },
                { "NMP Reduction",          new Parameter(  3,      1,      0,      100)    },
                { "NMP Reduction Divisor",  new Parameter(  6,      2,      1,      100)    },
                { "Futility Depth Margin",  new Parameter(  60,     50,     0,      1000)   },
                { "LMR Move Req",           new Parameter(  8,      10,     0,      100)    },
                { "LMR Depth Req",          new Parameter(  3,      1,      1,      20)     },
                { "LMR Reduction",          new Parameter(  3,      1,      1,      20)     },
            };
        }

        public Dictionary<string, Parameter> Parameters;
    }
}
