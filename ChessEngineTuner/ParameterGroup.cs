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
            public int MinDelta;
            public int MaxDelta;
            public int CurrentDelta;

            public int MinValue;
            public int MaxValue;

            public Parameter(int _value, int _minDelta, int _maxDelta, int _minValue, int _maxValue)
            {
                Value = _value;
                MinDelta = _minDelta;
                MaxDelta = _maxDelta;
                CurrentDelta = 0;

                MinValue = _minValue;
                MaxValue = _maxValue;
            }

            public Parameter(int _value)
            {
                Value = _value;
                MinDelta = 0;
                MaxDelta = Math.Max(_value / 10, 1);
                CurrentDelta = 0;

                MinValue = 1;
                MaxValue = Math.Max(_value * 3, 10);
            }

            public static implicit operator int(Parameter parameter) => parameter.Value;
            public static implicit operator Parameter(int _value) => new(_value);
        }
    }
}
