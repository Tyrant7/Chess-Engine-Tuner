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
            LMR = new Parameter(5, 0, 10, 30, 200);
        }

        public int Test;
        public Parameter LMR;
    }
}
