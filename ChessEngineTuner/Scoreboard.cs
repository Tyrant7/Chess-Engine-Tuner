using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    internal class Scoreboard
    {
        public readonly int[] Scores;

        public Scoreboard(int bots) 
        { 
            Scores = new int[bots];
        }

        public void ScoreMatch(int a, int b, int result)
        {
            Scores[a] += result;
            Scores[b] -= result;
        }
    }
}
