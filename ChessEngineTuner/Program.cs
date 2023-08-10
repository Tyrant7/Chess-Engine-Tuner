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
                Console.WriteLine(new String('=', 30));
                Console.WriteLine("Warning! Tuning from scratch will reset any weights currently in your engine directory. " +
                    "Please make sure you have a backup of your weights before continuing!");
                Console.WriteLine(new String('=', 30));
                Console.WriteLine("Press enter to begin tuning");
                Console.ReadLine();
            }

            Console.WriteLine("Starting tuning with {0} max matches...", matches);

            if (tuneFromScratch || !File.Exists(Settings.EvalFilePath))
            {
                // Write an empty set of parameters to the evaluation file
                EvaluationParameters parameters = new EvaluationParameters();
                parameters.WriteToFile(Settings.EvalFilePath);
            }

            for (int i = 0; i < matches; i++)
            {
                // TODO: Actually assign the weights
                // Read weights from eval file path in 2 unique Evaluation parameter objects
                // Make slight changes to each
                // Write back, one into file A and other into file B

                Process cutechess = CreateProcess();
                MatchResult result = RunMatch(cutechess);

                string winnerFile = "";
                switch (result)
                {
                    case MatchResult.BotAWins:
                        Console.WriteLine("Bot A wins");
                        winnerFile = "A-" + Settings.EvalFileName;
                        break;
                    case MatchResult.BotBWins:
                        winnerFile = "B-" + Settings.EvalFileName;
                        break;
                    case MatchResult.Draw:
                        Console.WriteLine("Draw");
                        winnerFile = Settings.EvalFileName; // No prepended A or B for a draw
                        break;
                    case MatchResult.Cancelled:
                        Console.WriteLine("Match was cancelled. Terminating process...");
                        return;
                }

                Console.WriteLine("Finished match {0}, adjusting weights accordingly...", i);

                // Copy the winner's weight data to be used as the base for the next iteration
                string winnerPath = Path.Combine(Settings.EngineDirectory, winnerFile);
                CopyParameters(winnerPath, Settings.EvalFilePath);
            }
            Console.WriteLine("Tuning session has concluded, you can find the results in " + Settings.EvalFilePath);
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
                        "-each proto=uci tc=1+0.08 bookdepth=6 book=./resources/book.bin -concurrency 10 -maxmoves 80 -games 2 -rounds 5000 " +
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

                    // TODO: Actually give some real win conditions
                    if (int.Parse(tokens[5]) >= 5)
                    {
                        return MatchResult.BotAWins;
                    }
                }
            }
            return MatchResult.Cancelled;
        }

        private static void CopyParameters(string fromPath, string toPath)
        {
            if (!File.Exists(fromPath))
            {
                Console.WriteLine("Could not copy weights from winner because the file was missing!");
                Console.WriteLine("Rolling winner's file back to previously saved version.");

                // Copy the other way in hopes to restore the winner's file back to the original weights
                if (File.Exists(toPath))
                    File.Copy(toPath, fromPath, true);

                return;
            }
            File.Copy(fromPath, toPath, true);
        }
    }
}