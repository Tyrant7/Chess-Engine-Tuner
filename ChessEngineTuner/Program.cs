using System;
using System.Collections;
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
                InitializeWeights(i);
                Process cutechess = CreateProcess();
                (int result, bool cancelled) = RunMatch(cutechess);

                Console.WriteLine();
                if (cancelled)
                {
                    Console.WriteLine("Match was cancelled. Terminating process...");
                    return;
                }
                else if (result == 0)
                {
                    Console.WriteLine("Match resulted in draw. Skipping adjustments.");
                    continue;
                }

                // Kill the current process after finished update
                cutechess.Kill(true);

                // Shift best parameters' raw values slightly towards the winning parameters
                ParameterGroup bestParameters = ParameterGroup.ReadFromFile(Settings.FilePath);
                foreach (var param in bestParameters.Parameters)
                {
                    // double change = deltas[param.Key] / ((double)Settings.GamesPerMatch * 2 / result) / 4;
                    // param.Value.RawValue = param.Value.RawValue + change;
                }
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
        private static void InitializeWeights(int match)
        {
            // Initialize our sets of weights for each bot
            for (int i = 0; i < Settings.BotsPerMatch; i++)
            {
                ParameterGroup newParameters = ParameterGroup.ReadFromFile(Settings.FilePath);

                Dictionary<string, double> deltas = new();

                int matchCycleIndex = match % Settings.CycleLength + 1;
                int cycleIndex = match / Settings.CycleLength + 1;

                // Make slight changes to each parameter
                var pars = newParameters.Parameters;
                foreach (KeyValuePair<string, ParameterGroup.Parameter> par in pars)
                {
                    ParameterGroup.Parameter newParam = pars[par.Key];
                    Random random = new Random();

                    // Value decreasing in magnitude towards target (gradient descent)
                    double delta = (newParam.MaxDelta / Math.Clamp(cycleIndex, 1, 8)) * Math.Exp(matchCycleIndex / (Settings.CycleLength * 2.0))
                        / Math.Sqrt(matchCycleIndex) * (Settings.CycleLength - matchCycleIndex) / Settings.CycleLength;

                    // Random positive or negative
                    delta *= random.Next(2) == 1 ? 1 : -1;

                    deltas.Add(par.Key, delta);
                    newParameters.Parameters[par.Key].RawValue = Math.Clamp(newParam.RawValue + delta, newParam.MinValue, newParam.MaxValue);
                }

                // Write back parameters into each file
                newParameters.WriteToFile(Settings.GetFilePath(i));
            }
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
        private static (int, bool) RunMatch(Process cutechess)
        {
            cutechess.Start();

            int gamesPlayed = 0;
            int gamesRemaining = Settings.GamesPerMatch * 2;
            while (!cutechess.StandardOutput.EndOfStream)
            {
                string line = cutechess.StandardOutput.ReadLine() ?? string.Empty;
                if (line.Contains("Finished game "))
                {
                    // Array will be formatted like
                    // Junk:  0-2, 4
                    // BotA:  3
                    // BotB:  5
                    // Score: 6
                    // Tidy up our input
                    string[] tokens = new string(line
                        .ToCharArray()
                        .Where(c => !char.IsPunctuation(c) || c == '-')
                        .ToArray())
                        .Split(' ');

                    foreach (string token in tokens)
                        Console.WriteLine(token);

                    /*

                    int botAWins = int.Parse(tokens[5]);
                    int botBWins = int.Parse(tokens[7]);
                    int draws = int.Parse(tokens[9]);

                    // Print our WDL
                    Console.WriteLine("BotA: {0}, BotB: {1}, Draws: {2}", botAWins, botBWins, draws);

                    gamesPlayed++;
                    gamesRemaining--;
                    if (gamesPlayed >= Settings.GamesPerMatch * 2)
                    {
                        return (botAWins - botBWins, false);
                    }
                    */
                }
            }
            Console.WriteLine("End");

            return (0, true);
        }
    }
}
