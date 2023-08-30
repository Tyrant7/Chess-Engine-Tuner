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
        public static readonly string EngineDirectory = "D:/Users/tyler/Chess-Challenge/Chess-Challenge/bin/Release/net6.0";

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
        /// The file path for bot weight files. Uses the engine directory.
        /// </summary>
        public static string GetFilePath(int botID)
            => Path.Combine(EngineDirectory, botID.ToString() + "-" + FileName);

        public const int DefaultMaxMatches = 1000;
        public const int BotsPerMatch = 4;
        public const int GamesPerMatch = 8;
        public const int ConcurrentGames = 6; // Ideally twice the value of GamesPerMatch

        // In seconds
        public const int GameTime = 1;
        public const double GameIncrement = 0.01;
    }
}
