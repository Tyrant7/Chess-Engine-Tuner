using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    static internal class Settings
    {
        /// <summary>
        /// The path of the file containing the weights to read from for evaluation. Put your original weights in here.
        /// Note: This file will be updated by the program as better weights are found. 
        /// </summary>
        public static readonly string CurrentEvalFilePath = "";
        /// <summary>
        /// The path of the file containing the weights to read from for search. Put your original weights in here.
        /// Note: This file will be updated by the program as better weights are found. 
        /// </summary>
        public static readonly string CurrentSearchFilePath = "";

        /// <summary>
        /// The directory where your bots will get their weights written to for testing. 
        /// Important Note: Should be in the same directory as your Chess-Challenge.exe, not this directory.
        /// </summary>
        public static readonly string EngineDirectory = "D:/Users/tyler/Chess-Challenge/Chess-Challenge/bin/Debug/net6.0";

        /// <summary>
        /// The name of the file containing your evaluation weights. Located in your engine directory.
        /// Additional files preprended with A and B be created for each bot respectively.
        /// </summary>
        public static readonly string EvalFileName = "Evaluation.weights";
        /// <summary>
        /// The name of the file containing your search weights. Located in your engine directory.
        /// Additional files preprended with A and B be created for each bot respectively.
        /// </summary>
        public static readonly string SearchFileName = "Search.weights";

        /// <summary>
        /// The full path of your evaluation weights.
        /// </summary>
        public static string EvalFilePath => Path.Combine(EngineDirectory, EvalFileName);
        /// <summary>
        /// The full path of your search weights.
        /// </summary>
        public static string SearchFilePath => Path.Combine(EngineDirectory, SearchFileName);

        public static string EvalFilePathA => Path.Combine(EngineDirectory, "A-" + EvalFileName);
        public static string SearchFilePathA => Path.Combine(EngineDirectory, "A-" + SearchFileName);
        public static string EvalFilePathB => Path.Combine(EngineDirectory, "B-" + EvalFileName);
        public static string SearchFilePathB => Path.Combine(EngineDirectory, "B-" + SearchFileName);

        public const int DefaultMaxMatches = 1000;
    }
}
