using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;

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
                bool fromScratch = args.Contains("fromscratch");
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
                Console.WriteLine("Warning! Tuning from scratch will reset any weights currently in this directory. " +
                    "Please make sure you have a backup of your weights before continuing!");
                Console.WriteLine(new string('=', 30));
                Console.WriteLine("Press enter to begin tuning");
                Console.ReadLine();
            }

            // Estimate how long tuning will take with the parameters given
            // 60,  average number of moves in a bot games (estimate)
            // 1.1,   time to start all processes of cutechess and ChessChallenge between games and margin of error
            int seconds = (int)Math.Round(matches * 1.1 * (Settings.GameTime * 2 + (Settings.GameIncrement * 120)));
            seconds = (int)(seconds * ((double)Settings.GamesPerMatch * 2 / Settings.ConcurrentGames));
            TimeSpan tuningTime = TimeSpan.FromSeconds(seconds);

            Console.WriteLine("Starting tuning with {0} max matches...", matches);
            Console.WriteLine("Estimated time: {0}\n", tuningTime);

            if (tuneFromScratch)
            {
                // Write a fully one set of parameters to the evaluation file
                ParameterGroup parameters = new ParameterGroup();
                parameters.OneOutParameters();
                parameters.WriteToFile(Settings.FilePath, true);
            }
            else if (!File.Exists(Settings.FilePath))
            {
                // Write a fresh set of parameters to the evaluation file
                ParameterGroup parameters = new ParameterGroup();
                parameters.WriteToFile(Settings.FilePath, true);

                Console.WriteLine("No previously created weights could be found. Initalized to default.");
            }
            else
            {
                Console.WriteLine("Began tuning using previously created weights.");
            }

            // Tuning analytics
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < matches; i++)
            {
                Console.WriteLine("Starting match {0} of {1}", i + 1, matches);
                (string changedParam, double[] deltas) = InitializeWeights(i);
                Process cutechess = CreateProcess();
                (Scoreboard results, bool cancelled) = RunMatch(cutechess);

                Console.WriteLine();
                if (cancelled)
                {
                    Console.WriteLine("Match was cancelled. Terminating process...");
                    return;
                }

                // Kill the current process after finished update
                cutechess.Kill(true);

                // Figure out who won and get their change
                int winner = results.GetWinner();
                double winningDelta = deltas[winner];

                // Copy over the winner's parameters to become the new best parameters, plus update the momentum value
                ParameterGroup bestParameters = ParameterGroup.ReadFromFile(Settings.FilePath);
                ParameterGroup.Parameter newParam = bestParameters.Parameters[changedParam];

                // Calculate the new momentum
                newParam.Momentum = Math.Abs(Math.Clamp(winningDelta / newParam.MaxDelta, 0, newParam.MaxDelta / 2));

                // Keep momentum away from zero
                if (Math.Abs(newParam.Momentum) <= 0.01)
                    newParam.Momentum = winningDelta < 0 ? -0.01 : 0.01;

                // Update the value and write to file
                newParam.RawValue = winningDelta;
                bestParameters.WriteToFile(Settings.FilePath, true);

                Console.WriteLine("Finished match. Adjusting weights according to winner...");
            }

            Console.WriteLine(new string('=', 30));
            Console.WriteLine("Tuning session has concluded in {0}.", stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
            Console.WriteLine("Final weights can be found at " + Settings.FilePath);
            Console.WriteLine(new string('=', 30));
        }

        /// <summary>
        /// Copies the weights files into separate files with slight adjustments for testing.
        /// </summary>
        /// <returns>The key of the modified parameter, as well as an array containing each modification for each bot.</returns>
        private static (string, double[]) InitializeWeights(int match)
        {
            // Initialize our sets of weights for each bot
            double[] deltas = new double[Settings.BotsPerMatch];

            // Setup parameters to read from by changing the active parameter
            ParameterGroup group = ParameterGroup.ReadFromFile(Settings.FilePath);
            int parameterIndex = match % group.Parameters.Count;

            string key = group.Parameters.ElementAt(parameterIndex).Key;
            ParameterGroup.Parameter par = group.Parameters[key];

            // Initialize each parameter inside of a range
            for (int i = 0; i < Settings.BotsPerMatch; i++)
            {
                // Even distribution between -MaxDelta and MaxDelta, clamped between min and max value for each parameter
                double delta = -par.MaxDelta + 2 * (double)par.MaxDelta / (Settings.BotsPerMatch - 1) * i;
                delta = Math.Clamp(delta, par.MinValue - par.RawValue, par.MaxValue - par.RawValue);
                par.RawValue += delta * par.Momentum;

                // Write back parameters into each file
                group.WriteToFile(Settings.GetFilePath(i));
                deltas[i] = delta;
            }

            return (key, deltas);
        }

        /// <summary>
        /// Starts a process of cutechess.
        /// </summary>
        /// <returns>A reference to the process.</returns>
        private static Process CreateProcess()
        {
            string Engine(int ID) => string.Format("-engine name=\"Bot{0}\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBot tune {0}\" ", ID);
            string GetEngines()
            {
                string output = string.Empty;
                for (int i = 0; i < Settings.BotsPerMatch; i++)
                    output += Engine(i);
                return output;
            }

            Process cutechess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Settings.EngineDirectory,
                    FileName = Settings.CutechessPath,
                    Arguments =
                        string.Format(
                        GetEngines() +
                        "-each proto=uci tc={0}+{1} bookdepth=6 book=./resources/book.bin -concurrency {2} -maxmoves 80 -games 2 -rounds {3} " +
                        "-ratinginterval 10 -pgnout games.pgn",
                        Settings.GameTime,
                        Settings.GameIncrement,
                        Settings.ConcurrentGames,
                        Settings.GamesPerMatch)
                }
            };

            // Kill cutechess once the program exits
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
            {
                if (!cutechess.HasExited)
                {
                    Console.WriteLine("Killing cutechess...");
                    cutechess.Kill(true);
                }
            };

            return cutechess;
        }

        /// <summary>
        /// Tracks a match between bots started in a cutechess process created by CreateProcess.
        /// </summary>
        /// <param name="cutechess">The process to track the match of.</param>
        /// <returns>The result of the match. + for bot A wins and - for bot B wins.</returns>
        private static (Scoreboard, bool) RunMatch(Process cutechess)
        {
            cutechess.Start();
            Scoreboard scoreboard = new Scoreboard(Settings.BotsPerMatch);

            int gamesPlayed = 0;
            while (!cutechess.StandardOutput.EndOfStream)
            {
                string line = cutechess.StandardOutput.ReadLine() ?? string.Empty;
                if (line.Contains("Finished game "))
                {
                    // Array will be formatted like
                    // Junk:  0-2, 4, 7+
                    // BotA:  3
                    // BotB:  5
                    // Score: 6
                    // Tidy up our input
                    string[] tokens = new string(line
                        .ToCharArray()
                        .Where(c => !char.IsPunctuation(c) || c == '-')
                        .ToArray())
                        .Split(' ');

                    // Figure out which 2 of our bots were playing and the score of the game
                    int botA = int.Parse(tokens[3].Replace("Bot", ""));
                    int botB = int.Parse(tokens[5].Replace("Bot", ""));

                    int score = tokens[6] == "1-0" ? 1 :
                                tokens[6] == "0-1" ? -1 :
                                0;

                    // Update the scoreboard with the new scores
                    scoreboard.UpdateScores(botA, botB, score);

                    gamesPlayed++;
                    if (gamesPlayed >= Settings.GamesPerMatch * 2)
                    {
                        // Print our scoreboard before we exit
                        scoreboard.Print();
                        return (scoreboard, false);
                    }
                }
            }
            Console.WriteLine("End");

            return (scoreboard, true);
        }
    }
}
