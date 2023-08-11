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
    public partial struct ParameterGroup
    {
        public ParameterGroup()
        {
            Test = 3;
            LMR = 7;
            GamePhase = new int[6];
        }

        public int Test;
        public Parameter<int> LMR;
        public Parameter<int[]> GamePhase;
    }
}
