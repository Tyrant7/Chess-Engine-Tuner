using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    internal class CSVWriter
    {
        public static void WriteToFile(Dictionary<string, ParameterGroup.Parameter> parameters, string path)
        {
            bool containsHeader = true;
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                containsHeader = false;
            }

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                if (!containsHeader)
                {
                    string header = string.Empty;
                    foreach (string key in parameters.Keys)
                    {
                        header += key + ",";
                    }
                    sw.WriteLine(header);
                }

                string line = string.Empty;
                foreach (ParameterGroup.Parameter parameter in parameters.Values)
                {
                    line += parameter.RawValue + ",";
                }
                sw.WriteLine(line);
                sw.Flush();
            }
        }
    }
}
