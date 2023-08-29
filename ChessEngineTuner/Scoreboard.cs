using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    internal class Scoreboard
    {
        private readonly int[] Scores;

        public Scoreboard(int bots) 
        {
            Scores = new int[bots];
        }

        public void UpdateScores(int a, int b, int result)
        {
            Scores[a] += result;
            Scores[b] -= result;
        }

        public int GetWinner()
        {
            return Scores.ToList().IndexOf(Scores.Max());
        }

        public void Print()
        {

        }
    }
}
