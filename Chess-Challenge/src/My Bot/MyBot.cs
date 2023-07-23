using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int searchDepth = 3;
    private Move bestMove;
    int[] pointValues = {100, 350, 350, 525, 1000, 99999};
    public Move Think(Board board, Timer timer)
    {
        Minimax(board, searchDepth, int.MinValue, int.MaxValue);
        return bestMove;
    }

    private int Minimax(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            int materialValue = 0;
            PieceList[] pieceLists = board.GetAllPieceLists();
            
            for(int i = 0;i < 5; i++){
                materialValue += (pieceLists[i].Count - pieceLists[i + 6].Count) * pointValues[i];
            }
            return materialValue;
        }

        int bestEval = board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        int moveEval;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            moveEval = Minimax(board, depth - 1, alpha, beta);
            board.UndoMove(move);

            if (board.IsWhiteToMove)
            {
                bestEval = Math.Max(bestEval, moveEval);
                alpha = Math.Max(alpha, moveEval);
            }
            else
            {
                bestEval = Math.Min(bestEval, moveEval);
                beta = Math.Min(beta, moveEval);
            }
            
            if (beta <= alpha)
            {
                break;
            }

            if (depth == searchDepth && moveEval == bestEval)
            {
                bestMove = move;
            }
        }
        
        return bestEval;
    }
}