using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata;

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
            // 1.8, number of matches with a winner that will be verified against bestParameters
            // 50,  average number of moves in a bot games (estimate)
            // 5,   time to start all processes of cutechess and ChessChallenge between games
            int seconds = (int)Math.Round(matches * 1.8 * 5 * Settings.GameTime + (Settings.GameIncrement * 50));
            seconds *= Settings.GamesPerMatch / Settings.ConcurrentGames;
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
            int updatedBest = 0;

            ParameterGroup bestParameters = ParameterGroup.ReadFromFile(Settings.FilePath);
            for (int i = 0; i < matches; i++)
            {
                Console.WriteLine("Starting match {0} of {1}", i + 1, matches);
                ParameterGroup botAParams, botBParams;
                (botAParams, botBParams) = InitializeWeights(i);
                Process cutechess = CreateProcess();
                MatchResult result = RunMatch(cutechess);

                // Update main parameter group to use next time based on winner
                ParameterGroup contender = new ParameterGroup();
                Console.WriteLine();
                switch (result)
                {
                    case MatchResult.BotAWins:
                        Console.WriteLine("Verifying bot A.");
                        contender = botAParams;
                        break;
                    case MatchResult.BotBWins:
                        Console.WriteLine("Verifying bot B.");
                        contender = botBParams;
                        break;
                    case MatchResult.Draw:
                        Console.WriteLine("Match resulted in draw. Skipping verifications.");
                        continue;
                    case MatchResult.Cancelled:
                        Console.WriteLine("Match was cancelled. Terminating process...");
                        return;
                }

                // Write contender for verification test
                contender.WriteToFile(Settings.FilePathB);

                // Kill original process and create new one for verification of new weights
                cutechess.Kill(true);
                cutechess = CreateProcess();

                // Write the best parameters to file A
                bestParameters.WriteToFile(Settings.FilePathA);
                result = RunMatch(cutechess);

                // Contender beat current best, update best weights
                if (result == MatchResult.BotBWins)
                {
                    Console.WriteLine("\nFound new best parameters. Updating main file.\n");

                    bestParameters = contender;
                    bestParameters.WriteToFile(Settings.FilePath, true);
                    updatedBest++;
                }
                else
                {
                    Console.WriteLine("\nDid not find new best parameters. Continuing games...\n");
                }

                // Kill the current process after finished update
                cutechess.Kill(true);
            }

            Console.WriteLine(new string('=', 30));
            Console.WriteLine("Tuning session has concluded in {0}.", stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
            Console.WriteLine("Updated best parameters a total of {0} times.", updatedBest);
            Console.WriteLine("Final weights can be found at " + Settings.FilePath);
            Console.WriteLine(new string('=', 30));
        }

        /// <summary>
        /// Copies the weights files into separate A and B files with slight adjustments for testing.
        /// </summary>
        /// <returns>Both ParameterGroups A and B.</returns>
        private static (ParameterGroup, ParameterGroup) InitializeWeights(int match)
        {
            // Initialize our two sets of weights
            ParameterGroup parameter_group = ParameterGroup.ReadFromFile(Settings.FilePath);
            ParameterGroup parametersA = ParameterGroup.ReadFromFile(Settings.FilePath);
            ParameterGroup parametersB = ParameterGroup.ReadFromFile(Settings.FilePath);

            int matchCycleIndex = match % Settings.CycleLength + 1;
            int cycleIndex = match / Settings.CycleLength + 1;

            // Make slight changes to each parameter
            var pars = parameter_group.Parameters;
            foreach (KeyValuePair<string, ParameterGroup.Parameter> par in pars)
            {
                ParameterGroup.Parameter newParam = pars[par.Key];
                Random random = new Random();

                // Value decreasing in magnitude towards target (gradient descent)
                int delta = (int)Math.Ceiling((newParam.MaxDelta / cycleIndex) * Math.Exp(matchCycleIndex / (Settings.CycleLength * 2.0)) 
                    / Math.Sqrt(matchCycleIndex) * (Settings.CycleLength - matchCycleIndex) / Settings.CycleLength);
                int sign = random.Next(2) == 1 ? 1 : -1;

                Console.WriteLine(delta);

                parametersA.Parameters[par.Key].Value = Math.Clamp(newParam.Value + (delta * sign), newParam.MinValue, newParam.MaxValue);
                parametersB.Parameters[par.Key].Value = Math.Clamp(newParam.Value - (delta * sign), newParam.MinValue, newParam.MaxValue);
            }

            // Write back, one into file A and other into file B
            parametersA.WriteToFile(Settings.FilePathA);
            parametersB.WriteToFile(Settings.FilePathB);

            return (parametersA, parametersB);
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
                        string.Format(
                        "-engine name=\"BotA\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBotA\" " +
                        "-engine name=\"BotB\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBotB\" " +
                        "-each proto=uci tc={0}+{1} bookdepth=6 book=./resources/book.bin -concurrency {2} -maxmoves 80 -games 2 -rounds {3} " +
                        "-ratinginterval 10 -pgnout games.pgn -sprt elo0=0 elo1=0 alpha=0.05 beta=0.05",
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
        /// <returns>The result of the match.</returns>
        private static MatchResult RunMatch(Process cutechess)
        {
            cutechess.Start();

            int gamesPlayed = 0;
            int gamesRemaining = Settings.GamesPerMatch;
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

                    int botAWins = int.Parse(tokens[5]);
                    int botBWins = int.Parse(tokens[7]);
                    int draws = int.Parse(tokens[9]);

                    // Print our WDL
                    Console.WriteLine("BotA: {0}, BotB: {1}, Draws: {2}", botAWins, botBWins, draws);

                    gamesPlayed++;
                    gamesRemaining--;
                    if (gamesPlayed >= Settings.GamesPerMatch)
                    {
                        int sumStats = botAWins - botBWins;
                        if (sumStats > 0)
                            return MatchResult.BotAWins;
                        else if (sumStats < 0)
                            return MatchResult.BotBWins;
                        else
                            return MatchResult.Draw;
                    }
                    // Determine if one bot has already won due to not having enough games for a comeback
                    else if (botAWins - botBWins > gamesRemaining)
                        return MatchResult.BotAWins;
                    else if (botBWins - botAWins > gamesRemaining)
                        return MatchResult.BotBWins;
                }
            }
            return MatchResult.Cancelled;
        }
    }
}
