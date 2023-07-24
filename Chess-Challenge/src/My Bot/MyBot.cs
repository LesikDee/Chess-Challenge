using ChessChallenge.API;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    static bool whiteSide = true;
    static bool initialized = false;
    string[] whitePremoves = new string[] { "e2e3", "g2g3", "g1e2", "f1g2", "e1g1" };
    static System.Random rnd;
    static Dictionary<PieceType, int[]> pieceValue = new Dictionary<PieceType, int[]>
    {
        {PieceType.Pawn, new int[]{10, 3 }},
        {PieceType.Knight, new int[]{30, 4 } },
        {PieceType.Bishop, new int[]{30, 4} },
        {PieceType.Rook, new int[]{50,2} },
        {PieceType.Queen, new int[]{100, 1} },
        {PieceType.King, new int[]{5, -1} }
    };


    public Move Think(Board board, Timer timer)
    {
        //System.Threading.Thread.Sleep(500);
        if (!initialized)
        {
            rnd = new System.Random();
            initialized = true;
        }
        whiteSide = board.IsWhiteToMove;

        System.Console.WriteLine(board.PlyCount);

        //return board.GetLegalMoves()[rnd.Next(moves.Length)];
        return minimaxMove(board);
    }

    private Move minimaxMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();


        Move bestMove = moves[0];
        var sideSign = (whiteSide ? 0 : 1) * 2 - 1;
        var bestValue = 9999 * sideSign;

        for (var i = 0; i < moves.Length; i++)
        {
            var move = moves[i];

            board.MakeMove(move);

            var boardValue = minimax(4, board, -10000, 10000, !whiteSide);

            boardValue += pieceValue[move.MovePieceType][1];
            board.UndoMove(move);
            if (whiteSide && (boardValue > bestValue))
            {
                bestValue = boardValue;
                bestMove = move;
            }
            else if (!whiteSide && (boardValue < bestValue))
            {
                bestValue = boardValue;
                bestMove = move;
            }
            System.Console.WriteLine(move.ToString() + " " + boardValue + " " + bestValue);
        }

        return bestMove;
    }

    private int minimax(int depth, Board board, int alpha, int beta, bool isMaximisingPlayer)
    {
        if (depth == 0)
        {
            return evaluateBoard(board);
        }
        Move[] moves = board.GetLegalMoves();
        if (isMaximisingPlayer)
        {
            var bestValue = -9999;
            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i]);
                bestValue = System.Math.Max(bestValue, minimax(depth - 1, board, alpha, beta, !isMaximisingPlayer));
                board.UndoMove(moves[i]);
                alpha = System.Math.Max(alpha, bestValue);
                if (beta <= alpha)
                {
                    return bestValue;
                }
            }
            return bestValue;
        }
        else
        {
            var bestValue = 9999;
            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i]);
                bestValue = System.Math.Min(bestValue, minimax(depth - 1, board, alpha, beta, !isMaximisingPlayer));
                board.UndoMove(moves[i]);
                beta = System.Math.Min(beta, bestValue);
                if (beta <= alpha)
                {
                    return bestValue;
                }
            }
            return bestValue;
        }
    }


    private int evaluateBoard(Board board)
    {
        PieceList[] pieces = board.GetAllPieceLists();
        int count = 0;
        foreach (var piece in pieces)
        {
            var a = pieceValue[piece.TypeOfPieceInList][0] * piece.Count;
            var b = (piece.IsWhitePieceList ? 1 : 0) * 2 - 1;
            count += a * b;
            // count += pieceValue[piece.TypeOfPieceInList] * ((piece.IsWhitePieceList ? 0 : 1) * 2 - 1);
        }
        //System.Console.WriteLine(count);
        return count;
    }
}