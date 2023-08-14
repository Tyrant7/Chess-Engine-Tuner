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
        /// The build directory of your bots. 
        /// Important Note: Should be in the same directory as your Chess-Challenge.exe, not this directory.
        /// </summary>
        public static readonly string EngineDirectory = "D:/Users/tyler/Chess-Challenge/Chess-Challenge/bin/Debug/net6.0";

        /// <summary>
        /// The path of your Cutechess-cli.exe file.
        /// </summary>
        public static readonly string CutechessPath = "D:/Users/tyler/AppData/Local/Programs/Cute Chess/cutechess-cli.exe";

        /// <summary>
        /// The name of the file containing your evaluation weights. Located in your engine directory.
        /// Additional files preprended with A and B be created for each bot respectively.
        /// </summary>
        private static readonly string FileName = "Evaluation.weights";

        /// <summary>
        /// The full path of your evaluation weights.
        /// </summary>
        public static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), FileName);

        /// <summary>
        /// The file path for Bot A. Uses the engine directory.
        /// </summary>
        public static string FilePathA => Path.Combine(EngineDirectory, "A-" + FileName);
        /// <summary>
        /// The file path for Bot A. Uses the engine directory.
        /// </summary>
        public static string FilePathB => Path.Combine(EngineDirectory, "B-" + FileName);

        public const int DefaultMaxMatches = 1000;
        public const int GamesPerMatch = 4; // Played by each side
        public const int ConcurrentGames = 8; // Ideally twice the value of GamesPerMatch
        public const int CycleLength = 20; // The length of a cycle for delta

        // In seconds
        public const int GameTime = 8;
        public const double GameIncrement = 0.08;
    }
}
