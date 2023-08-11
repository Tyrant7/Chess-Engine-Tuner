using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    public partial struct ParameterGroup
    {
        static private JsonSerializerOptions Options => new()
        {
            WriteIndented = true,
            IncludeFields = true
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

        public struct Parameter<T>
        {
            public readonly T Value;
            public readonly int MinDelta;
            public readonly int MaxDelta;

            public Parameter(T val)
            {
                Value = val;
                MinDelta = 1;
                MaxDelta = 5;
            }

            public static implicit operator T(Parameter<T> parameter) => parameter.Value;
            public static implicit operator Parameter<T>(T value) => new Parameter<T>(value);
        }
    }
}
