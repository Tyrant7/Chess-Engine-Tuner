using System;
using System.Diagnostics;

namespace ChessEngineTuner
{
    public static class Program
    {
        private static Process cutechess;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                BeginTuning(1000);
            }
            else if (args.Length == 2 && args[0].Contains("maxgames"))
            {
                if (int.TryParse(args[1], out int games))
                {
                    BeginTuning(games);
                    return;
                }
            }
            Console.WriteLine("Improper format. Please input 'maxgames <number>'");
        }

        private static void BeginTuning(int maxGames)
        {
            Console.WriteLine("Starting tuning with {0} max games...", maxGames);

            cutechess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = "D:/Users/tyler/Chess-Challenge/Chess-Challenge/bin/Debug/net6.0",
                    FileName = "D:/Users/tyler/AppData/Local/Programs/Cute Chess/cutechess-cli.exe",
                    Arguments =
                        "-engine name=\"V5.1 Tuned\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBot\" " +
                        "-engine name=\"V5.1 Tuned\" cmd=\"./Chess-Challenge.exe\" arg=\"cutechess uci TunedBot\" " +
                        "-each proto=uci tc=8+0.08 bookdepth=6 book=./resources/book.bin -concurrency 1 -maxmoves 80 -games 2 -rounds 5000 " +
                        "-ratinginterval 10 -pgnout games.pgn -sprt elo0=0 elo1=20 alpha=0.05 beta=0.05"
                }
            };
            cutechess.Start();

            // Kill cutechess once the program exits
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => {
                Console.WriteLine("Killing cutechess...");
                cutechess.Kill(true);
            };

            while (!cutechess.StandardOutput.EndOfStream)
            {
                string line = cutechess.StandardOutput.ReadLine() ?? string.Empty;
                Console.WriteLine(line);
            }
        }
    }
}