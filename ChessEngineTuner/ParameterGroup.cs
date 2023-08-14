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

        /// <summary>
        /// Writes this object to a .weights file in JSON notation.
        /// </summary>
        /// <param name="path">The full path to write to.</param>
        /// <param name="writeRaw">Should we write the full Parameter data or only the weights?</param>
        public void WriteToFile(string path, bool verbose = false)
        {
            // Write parameter group data in JSON format
            string myJsonData;
            if (!verbose)
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

        /// <summary>
        /// Returns a ParameterGroup read from the specified file.
        /// </summary>
        /// <param name="path">The full path of the file to read from.</param>
        /// <returns>A newly created ParameterGroup object</returns>
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
                Parameters[param.Key].RawValue = 1;
            }
        }

        public struct RawParameterGroup
        {
            public RawParameterGroup(ParameterGroup group)
            {
                Parameters = new Dictionary<string, int>(group.Parameters.Count);
                foreach (KeyValuePair<string, ParameterGroup.Parameter> par in group.Parameters)
                {
                    Parameters.Add(par.Key, par.Value.Value);
                }
            }

            public Dictionary<string, int> Parameters;
        }

        public class Parameter
        {
            public int Value => (int)Math.Floor(RawValue);
            public double RawValue;

            public int MaxDelta;

            public int MinValue;
            public int MaxValue;

            // Required definition for deserialization
            public Parameter() { }

            public Parameter(int value, int maxDelta, int minValue, int maxValue)
            {
                RawValue = value;
                MaxDelta = maxDelta;

                MinValue = minValue;
                MaxValue = maxValue;
            }

            public Parameter(int value)
            {
                RawValue = value;
                MaxDelta = Math.Max(value / 2, 1);

                MinValue = 1;
                MaxValue = Math.Max(value * 3, 10);
            }

            public static implicit operator int(Parameter parameter) => parameter.Value;
        }
    }
}
