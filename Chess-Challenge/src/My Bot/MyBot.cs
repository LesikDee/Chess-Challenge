using ChessChallenge.API;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    static int depth = 5;
    static bool whiteSide = true;
    static bool initialized = false;
    string[] whitePremoves = new string[] { "e2e3", "g2g3", "g1e2", "f1g2", "e1g1" };
    static System.Random rnd;
    static int countN = 0;
    static Dictionary<PieceType, double[]> pieceValue = new Dictionary<PieceType, double[]> // 86
    {
        {PieceType.Pawn, new double[]{10, 3 }},
        {PieceType.Knight, new double[]{30, 4 } },
        {PieceType.Bishop, new double[]{30, 0.9} },
        {PieceType.Rook, new double[]{50, 0}},
        {PieceType.Queen, new double[]{100, 0} },
        {PieceType.King, new double[]{5, -1} }
    };
    int[] a = new int[] { 0, 10, 30, 30, 50, 100, 5 }; // 18
    static Dictionary<PieceType, double[][]> mitterspielEvalTable = new Dictionary<PieceType, double[][]>
    {
        //{
        //    PieceType.King, new double[][] {
        //        new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        //        new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        //        new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        //        new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        //        new double[]{-2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0 },
        //        new double[]{-1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0 },
        //        new double[]{2.0, 2.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2},
        //        new double[]{2.0, 0, 30.0, 0.0, 0.0, 0.0, 30.0, 2 },
        //    }
        //},
        {
              PieceType.Knight, new double[][] {
                new double[]{-2.5, -2.0, -1.5, -1.5, -3.0, -1.5, -2.0, -2.5 },
                new double[]{-2.0, -2.0, 0.0, 0.0, 0.0, 0.0, -2.0,-2.0 },
                new double[]{-1.5, 0.0, 1.0, 1.5, 1.5, 1.0, 0, -1.5  },
                new double[]{-1.5, 0.5, 1.5, 2.0, 2.0, 1.5, 0.5, -1.5 },
                new double[]{-1.5, 0.5, 1.5, 2.0, 2.0, 1.5, 0.5, -1.5 },
                new double[]{-1.5, 0.0, 1.0, 1.5, 1.5, 1.0, 0, -1.5  },
                new double[]{-2.0, -2.0, 0.0, 0.0, 0.0, 0.0, -2.0,-2.0 },
                new double[]{-2.5, -2.0, -1.5, -1.5, -3.0, -1.5, -2.0, -2.5 },
            }
        },
        {
              PieceType.Pawn, new double[][] {
                new double[]{0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
                new double[]{5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0},
                new double[]{1.0,  1.0,  2.0,  3.0,  3.0,  2.0,  1.0,  1.0},
                new double[]{0.5,  0.5,  1.0,  2.5,  2.5,  1.0,  0.5,  0.5},
                new double[]{0.0,  0.0,  0.0,  2.0,  2.0,  0.0,  0.0,  0.0},
                new double[]{0.5, -0.5, -1.0,  0.0,  0.0, -1.0, -0.5,  0.5},
                new double[]{0.5,  1.0, 1.0,  -2.0, -2.0,  1.0,  1.0,  0.5},
                new double[]{0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0}
            }
        }
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
        double bestValue = 9999 * sideSign;

        for (var i = 0; i < moves.Length; i++)
        {
            var move = moves[i];

            board.MakeMove(move);

            double boardValue = minimax(depth, board, -10000, 10000, !whiteSide);

            //if (board.PlyCount < 320)
            //{
            //    if (mitterspielEvalTable.ContainsKey(move.MovePieceType))
            //    {
            //        boardValue += mitterspielEvalTable[move.MovePieceType]
            //            [whiteSide ? 7 - move.TargetSquare.Rank : move.TargetSquare.Rank][move.TargetSquare.File] * sideSign * -1;
            //    }
            //    else
            //    {
            //        var moveStr = move.ToString();
            //        if (move.MovePieceType == PieceType.King && (moveStr.Equals("Move: 'e8g8'") || moveStr.Equals("Move: 'e8c8'")))
            //            boardValue += 5 * sideSign * -1;
            //        boardValue += pieceValue[move.MovePieceType][1] * sideSign * -1;
            //    }
            //}
            //else
            //{
            //    boardValue += pieceValue[move.MovePieceType][1] * sideSign * -1;
            //}

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
        System.Console.WriteLine("countN " + countN);
        countN = 0;
        return bestMove;
    }

    private double minimax(int depth, Board board, double alpha, double beta, bool isMaximisingPlayer)
    {
        if (depth == 0)
        {
            return evaluateBoard(board);
        }
        Move[] moves = board.GetLegalMoves();
        if (isMaximisingPlayer)
        {
            var bestValue = -9999.0;
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
            var bestValue = 9999.0;
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


    private double evaluateBoard(Board board)
    {
        PieceList[] pieces = board.GetAllPieceLists();
        double count = 0;
        foreach (var piece in pieces)
        {
            //var a = pieceValue[piece.TypeOfPieceInList][0] * piece.Count;
            //var b = (piece.IsWhitePieceList ? 1 : 0) * 2 - 1;
            //count += pieceValue[piece.TypeOfPieceInList][0] * piece.Count * ((piece.IsWhitePieceList ? 1 : 0) * 2 - 1);
            for (int i = 0; i < piece.Count; i++)
            {
                countN++;
                count += evaluatePiecePosition(piece.GetPiece(i), board.IsWhiteToMove);
            }
            // count += pieceValue[piece.TypeOfPieceInList] * ((piece.IsWhitePieceList ? 0 : 1) * 2 - 1);
        }
        return count;
    }
    private double evaluatePiecePosition(Piece piece, bool isWhiteToMove)
    {
        if (mitterspielEvalTable.ContainsKey(piece.PieceType))
        {
           
            var a = mitterspielEvalTable[piece.PieceType]
                [piece.IsWhite ? 7 - piece.Square.Rank : piece.Square.Rank][piece.Square.File] * ((piece.IsWhite ? 1 : 0) * 2 - 1);
            //System.Console.WriteLine(piece.ToString() + piece.Square.ToString() + a);
            return a;
        }
        return 0;
    }
}