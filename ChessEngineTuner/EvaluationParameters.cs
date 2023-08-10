using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngineTuner
{
    public struct EvaluationParameters : IParameter
    {
        // Size 6
        private readonly int[] GamePhaseIncrement;

        // Size 12
        private static readonly short[] PieceValues;

        // All size 64
        private static int[] mg_pawn_table;
        private static int[] eg_pawn_table;

        private static int[] mg_knight_table;
        private static int[] eg_knight_table;

        private static int[] mg_bishop_table;
        private static int[] eg_bishop_table;

        private static int[] mg_rook_table;
        private static int[] eg_rook_table;

        private static int[] mg_queen_table;
        private static int[] eg_queen_table;

        private static int[] mg_king_table;
        private static int[] eg_king_table;


        public void WriteToFile(string path)
        {
            // TODO: Write in JSON format
            throw new NotImplementedException();
        }

        public void ReadFromFile(string path)
        {
            // TODO: Readback and initialize
            throw new NotImplementedException();
        }
    }
}
