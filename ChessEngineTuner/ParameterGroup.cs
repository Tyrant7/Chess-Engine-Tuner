using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        public void WriteToFile(string path, bool writeRaw = true)
        {
            // Write parameter group data in JSON format
            string myJsonData;
            if (writeRaw)
            {
                RawParameterGroup rawParams = new RawParameterGroup(this);
                myJsonData = JsonSerializer.Serialize(rawParams, Options);
            }
            else
            {
                myJsonData = JsonSerializer.Serialize(this, Options);
            }
            File.WriteAllText(path, myJsonData);
        }

        public static ParameterGroup ReadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("There was no file at the specified path: {0}\nCreating one now!", path);
                return new ParameterGroup();
            }

            // Read all data from the file and creates a new parameter group
            string jsonData = File.ReadAllText(path);
            ParameterGroup group = JsonSerializer.Deserialize<ParameterGroup>(jsonData, Options);
            return group;
        }

        /// <summary>
        /// Set every parameter in this group to 1.
        /// </summary>
        public void OneOutParameters()
        {
            foreach (var param in Parameters)
            {
                Parameters[param.Key] = 1;
            }
        }

        public struct Parameter
        {
            public int Value;
            public int Temp;
            public int Min_Value;
            public int Max_Value;
            public int delta;
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
                delta = 0;
                Progress_1 = 0.0;
                Progress_2 = 0.0;
                R = -1.0;
                a = Math.Clamp(_Value, 2, 30);
                c0 = Math.Clamp(_Value * 3, 5, 200);
                c = Math.Clamp(_Value * 3, 5, 200);
                corr = 1.0;
            }

            public Parameter(int _Value)
            {
                Value = _Value;
                Temp = _Value;
                Min_Value = 1;
                Max_Value = Math.Max(_Value, 3) * 3;
                delta = 0;
                Progress_1 = 0.0;
                Progress_2 = 0.0;
                R = -1.0;
                a = Math.Clamp(_Value, 2, 30);
                c0 = Math.Clamp(_Value * 3, 5, 200);
                c = Math.Clamp(_Value * 3, 5, 200);
                corr = 1.0;
            }

            public static implicit operator int(Parameter parameter) => parameter.Value;
            public static implicit operator Parameter(int _value) => new(_value);
        }
    }
}
