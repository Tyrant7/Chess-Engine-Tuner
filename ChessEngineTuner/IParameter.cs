using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    internal interface IParameter
    {
        public void WriteToFile(string path);
        public void ReadFromFile(string path);
    }
}
