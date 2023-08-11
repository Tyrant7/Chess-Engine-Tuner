using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
            AWWiden = 65;
            AWSize = 20;
            RFPMargin = 100;
            NMP_R = 3;
            NMPDepthCoef = 5;
            EFPMargin = 120;
            LMR_R = 3;
            LMRDepthMargin = 3;
            LMRTriedMargin = 8;
        }

        public Parameter AWWiden;
        public Parameter AWSize;
        public Parameter RFPMargin;
        public Parameter NMP_R;
        public Parameter NMPDepthCoef;
        public Parameter EFPMargin;
        public Parameter LMR_R;
        public Parameter LMRDepthMargin;
        public Parameter LMRTriedMargin;
    }
}
