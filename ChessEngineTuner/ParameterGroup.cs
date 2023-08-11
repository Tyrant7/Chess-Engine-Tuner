using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    public partial struct ParameterGroup
    {
        static private JsonSerializerOptions Options => new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        public void WriteToFile(string path)
        {
            // Write parameter group data in JSON format
            string myJsonData = JsonSerializer.Serialize(this, Options);
            File.WriteAllText(path, myJsonData);
        }

        public static ParameterGroup ReadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("There was no file at the specified path: {0}", path);
                return new ParameterGroup();
            }

            // Read all data from the file and creates a new parameter group
            string jsonData = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ParameterGroup>(jsonData, Options);
        }

        public struct Parameter
        {
            public int Value;
            public int Temp;
            public int Min_Value;
            public int Max_Value;
            public double Progress_1;
            public double Progress_2;
            public double R;
            public double a;
            public double c0;
            public double c;
            public double corr;

            public Parameter(int _Value, int _Min_Value, int _Max_Value)
            {
                Value = _Value;
                Temp = _Value;
                Min_Value = _Min_Value;
                Max_Value = _Max_Value;
                Progress_1 = 0.0;
                Progress_2 = 0.0;
                R = -1.0;
                a = 30.0;
                c0 = 200.0;
                c = 200.0;
                corr = 1.0;
            }

            public Parameter(int _Value)
            {
                Value = _Value;
                Temp = _Value;
                Min_Value = -1000000;
                Max_Value = 1000000;
                Progress_1 = 0.0;
                Progress_2 = 0.0;
                R = -1.0;
                a = 30.0;
                c0 = 200.0;
                c = 200.0;
                corr = 1.0;
            }

            public static implicit operator int(Parameter parameter) => parameter.Value;
            public static implicit operator Parameter(int _value) => new(_value);
        }
    }
}
