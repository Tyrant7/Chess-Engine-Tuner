using System;
using System.Diagnostics;

namespace ChessEngineTuner
{
    public static class Program
    {
        private enum MatchResult
        {
            BotAWins, BotBWins, Draw, Cancelled
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                BeginTuning(Settings.DefaultMaxMatches, false);
            }
            else if (args.Length > 0 && args[0].Contains("maxmatches"))
            {
                bool fromScratch = (args.Length >= 2) && args[2].Contains("fromscratch");
                if (int.TryParse(args[1], out int matches))
                {
                    BeginTuning(matches, fromScratch);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Improper format. Please use 'maxmatches { 1000 } [fromscratch]'");
            }
        }

        /// <summary>
        /// Tunes the bots for the specified number of matches.
        /// </summary>
        /// <param name="matches">The number of matches to tune for.</param>
        private static void BeginTuning(int matches, bool tuneFromScratch)
        {
            if (tuneFromScratch)
            {
                Console.WriteLine(new string('=', 30));
                Console.WriteLine("Warning! Tuning from scratch will reset any weights currently in your engine directory. " +
                    "Please make sure you have a backup of your weights before continuing!");
                Console.WriteLine(new string('=', 30));
                Console.WriteLine("Press enter to begin tuning");
                Console.ReadLine();
            }

            Console.WriteLine("Starting tuning with {0} max matches...", matches);

            if (tuneFromScratch || !File.Exists(Settings.FilePath))
            {
                // Write an empty set of parameters to the evaluation file
                ParameterGroup parameters = new ParameterGroup();
                parameters.WriteToFile(Settings.FilePath);
            }

            for (int i = 0; i < matches; i++)
            {
                InitializeWeights();
                Process cutechess = CreateProcess();
                MatchResult result = RunMatch(cutechess);

                string winnerFile = "";
                switch (result)
                {
                    case MatchResult.BotAWins:
                        Console.WriteLine("Bot A wins");
                        winnerFile = Settings.FilePathA;
                        break;
                    case MatchResult.BotBWins:
                        winnerFile = Settings.FilePathB;
                        break;
                    case MatchResult.Draw:
                        Console.WriteLine("Draw");
                        winnerFile = Settings.FilePath; // No prepended A or B for a draw, just use the default file
                        break;
                    case MatchResult.Cancelled:
                        Console.WriteLine("Match was cancelled. Terminating process...");
                        return;
                }

                Console.WriteLine("Finished match {0}, adjusting weights accordingly...", i);
            }
            Console.WriteLine("Tuning session has concluded, you can find the results in " + Settings.FilePath);
        }

        /// <summary>
        /// Copies the weights files into separate A and B files with slight adjustments for testing.
        /// </summary>
        private static void InitializeWeights()
        {
            // Initialize our two sets of weights
            ParameterGroup parametersA = ParameterGroup.ReadFromFile(Settings.FilePath);
            ParameterGroup parametersB = ParameterGroup.ReadFromFile(Settings.FilePath);

            // TODO: Make slight changes to each

            // Write back, one into file A and other into file B
            parametersA.WriteToFile(Settings.FilePathA);
            parametersB.WriteToFile(Settings.FilePathB);
        }

        /// <summary>
        /// Starts a process of cutechess.
        /// </summary>
        /// <returns>A reference to the process.</returns>
        private static Process CreateProcess()
        {
            Process cutechess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Settings.EngineDirectory,
                    FileName = "D:/Users/tyler/AppData/Local/Programs/Cute Chess/cutechess-cli.exe",
                    Arguments =
                        // Put your command to CuteChess here
                        "-engine name=\"BotA\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBot\" " +
                        "-engine name=\"BotB\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBot\" " +
                        "-each proto=uci tc=1+0.08 bookdepth=6 book=./resources/book.bin -concurrency 2 -maxmoves 80 -games 2 -rounds 1 " +
                        "-ratinginterval 10 -pgnout games.pgn -sprt elo0=0 elo1=20 alpha=0.05 beta=0.05"
                }
            };

            // Kill cutechess once the program exits
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => {
                Console.WriteLine("Killing cutechess...");
                cutechess.Kill(true);
            };

            return cutechess;
        }

        /// <summary>
        /// Tracks a match between bots started in a cutechess process created by CreateProcess.
        /// </summary>
        /// <param name="cutechess">The process to track the match of.</param>
        /// <returns>The result of the match.</returns>
        private static MatchResult RunMatch(Process cutechess)
        {
            cutechess.Start();

            int gamesPlayed = 0;
            while (!cutechess.StandardOutput.EndOfStream)
            {
                string line = cutechess.StandardOutput.ReadLine() ?? string.Empty;
                if (line.Contains("Score of BotA vs BotB: "))
                {
                    // Array will be formatted like
                    // Junk: 0-4
                    // BotA: 5
                    // BotB: 7
                    // Draw: 9
                    string[] tokens = line.Split(' ');

                    // Print our WDL
                    Console.WriteLine("BotA: {0}, BotB: {1}, Draws: {2}", tokens[5], tokens[7], tokens[9]);
                    gamesPlayed++;

                    if (gamesPlayed >= 2)
                    {
                        int sumStats = int.Parse(tokens[5]) - int.Parse(tokens[7]);
                        if (sumStats > 0)
                            return MatchResult.BotAWins;
                        else if (sumStats < 0)
                            return MatchResult.BotBWins;
                        else
                            return MatchResult.Draw;
                    }
                }
            }
            return MatchResult.Cancelled;
        }
    }
}