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
                Console.WriteLine("Warning! Tuning from scratch will reset any weights currently in your engine directory. " +
                    "Please make sure you have a backup of your weights before continuing!");
                Console.WriteLine(new string('=', 30));
                Console.WriteLine("Press enter to begin tuning");
                Console.ReadLine();
            }

            Console.WriteLine("Starting tuning with {0} max matches...", matches);

            if (tuneFromScratch)
            {
                // Write a zeroed out set of parameters to the evaluation file
                ParameterGroup parameters = new ParameterGroup();
                parameters.ZeroOutParameters();
                parameters.WriteToFile(Settings.FilePath, false);
            }
            else if (!File.Exists(Settings.FilePath))
            {
                // Write a fresh set of parameters to the evaluation file
                ParameterGroup parameters = new ParameterGroup();
                parameters.WriteToFile(Settings.FilePath, false);
            }

            for (int i = 0; i < matches; i++)
            {
                InitializeWeights(i, matches);
                Process cutechess = CreateProcess();
                (MatchResult result, int score) = RunMatch(cutechess);

                switch (result)
                {
                    case MatchResult.BotAWins:
                        Console.WriteLine("Bot A wins");
                        break;
                    case MatchResult.BotBWins:
                        break;
                    case MatchResult.Draw:
                        Console.WriteLine("Draw");
                        break;
                    case MatchResult.Cancelled:
                        Console.WriteLine("Match was cancelled. Terminating process...");
                        return;
                }

                Console.WriteLine("Finished match {0}, adjusting weights accordingly...", i);
                cutechess.Kill();

                // Update main parameter group to use next time based on winner
                ParameterGroup parameter_group = ParameterGroup.ReadFromFile(Settings.FilePath);
                var p = parameter_group.Parameters;
                foreach (var param in p)
                {
                    p[param.Key] += (int)(p[param.Key].a * score / (p[param.Key].c * p[param.Key].delta));
                    p[param.Key] = Math.Clamp(p[param.Key].Value, p[param.Key].Min_Value, p[param.Key].Max_Value);
                }
                parameter_group.WriteToFile(Settings.FilePath, false);

            }
            Console.WriteLine("Tuning session has concluded, you can find the results in " + Settings.FilePath);
        }

        /// <summary>
        /// Copies the weights files into separate A and B files with slight adjustments for testing.
        /// </summary>
        private static void InitializeWeights(int match, int matches)
        {
            // Initialize our two sets of weights
            ParameterGroup parameter_group = ParameterGroup.ReadFromFile(Settings.FilePath);
            ParameterGroup parametersA = new(), parametersB = new();

            // Make slight changes to each
            int A = 5000;


            var pars = parameter_group.Parameters;
            foreach (KeyValuePair<string, ParameterGroup.Parameter> par in pars)
            {
                ParameterGroup.Parameter newParam = pars[par.Key];
                if (match == A)
                {
                    newParam.Progress_1 = Math.Abs(newParam.Value - newParam.Temp);
                    newParam.Temp = newParam.Value;
                }

                if (match > A && match % A == 0)
                {
                    newParam.Progress_2 = Math.Abs(newParam.Value - newParam.Temp);
                    newParam.R = newParam.Progress_1 > 0.001 ? newParam.Progress_2 / (newParam.corr * newParam.Progress_1) : -1.0;

                    if (newParam.R > 1.0e-6 && newParam.R < 0.999999)
                    {
                        newParam.corr = Math.Clamp(-2.0 * A / (matches * Math.Log(newParam.R)), 0.8, 1.25);
                    }
                    if (newParam.R <= 1.0e-6) { newParam.corr = 0.8; }
                    if (newParam.R >= 0.999999) { newParam.corr = 1.25; }
                    if (newParam.R == -1.0) { newParam.corr = 1.0; }

                    newParam.a *= newParam.corr;
                    newParam.Progress_1 = newParam.Progress_2;
                    newParam.Temp = newParam.Value;
                }

                newParam.c = newParam.c0 * Math.Exp(2.0 * (match + 1) / matches) / (match + 1);
                newParam.delta = new Random().Next(2) * 2 - 1;

                pars[par.Key] = newParam;
                parametersA.Parameters[par.Key] = Math.Clamp((int)(newParam.Value + newParam.c * newParam.delta), newParam.Min_Value, newParam.Max_Value);
                parametersB.Parameters[par.Key] = Math.Clamp((int)(newParam.Value - newParam.c * newParam.delta), newParam.Min_Value, newParam.Max_Value);
            }

            parameter_group.WriteToFile(Settings.FilePath, false);

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
                        "-each proto=uci tc=3 bookdepth=6 book=./resources/book.bin -concurrency 2 -maxmoves 80 -games 2 -rounds 1 " +
                        "-ratinginterval 10 -pgnout games.pgn -sprt elo0=0 elo1=20 alpha=0.05 beta=0.05"
                }
            };

            // Kill cutechess once the program exits
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
            {
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
        private static (MatchResult, int) RunMatch(Process cutechess)
        {
            cutechess.Start();

            int gamesPlayed = 0;
            while (!cutechess.StandardOutput.EndOfStream)
            {
                string line = cutechess.StandardOutput.ReadLine() ?? string.Empty;
                Console.WriteLine(line);
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
                            return (MatchResult.BotAWins, sumStats);
                        else if (sumStats < 0)
                            return (MatchResult.BotBWins, sumStats);
                        else
                            return (MatchResult.Draw, sumStats);
                    }
                }
            }
            return (MatchResult.Cancelled, 0);
        }
    }
}