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
        /// <param name="writeRaw">If true, will only write a dictionary containing the integer values of the Parameters and not all data.</param>
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
                Parameters[param.Key].Value = 1;
            }
        }

        public class Parameter
        {
            public int Value { get; set; }
            public int MinDelta;
            public int MaxDelta;

            public int MinValue;
            public int MaxValue;

            // Required definition for deserialization
            public Parameter() { }

            public Parameter(int value, int minDelta, int maxDelta, int minValue, int maxValue)
            {
                Value = value;
                MinDelta = minDelta;
                MaxDelta = maxDelta;

                MinValue = minValue;
                MaxValue = maxValue;
            }

            public Parameter(int value)
            {
                Value = value;
                MinDelta = 0;
                MaxDelta = Math.Max(value / 10, 1);

                MinValue = 1;
                MaxValue = Math.Max(value * 3, 10);
            }

            public static implicit operator int(Parameter parameter) => parameter.Value;
        }
    }
}
