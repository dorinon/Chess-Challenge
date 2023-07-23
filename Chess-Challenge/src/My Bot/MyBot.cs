using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int searchDepth = 3;
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    int[] pointValues = {100, 350, 350, 525, 1000, 99999};
    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        // Call the Minimax algorithm to find the best move
        Minimax(searchDepth, int.MinValue, int.MaxValue);
        
        return bestMove;
    }

    // Minimax algorithm with alpha-beta pruning
    private int Minimax(int depth, int alpha, int beta)
    {
        // If the search reaches the desired depth or the end of the game, evaluate the position and return its value
        if (depth == 0)
        {
            return EvaluateBoard();
        }

        int bestEval = board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        int moveEval;
        
        // Generate and loop through all legal moves for the current player
        foreach (Move move in board.GetLegalMoves())
        {
            // Make the move on a temporary board and call Minimax recursively
            board.MakeMove(move);
            moveEval = Minimax(depth - 1, alpha, beta);
            board.UndoMove(move);

            // Update the best evaluation and alpha/beta values based on whether it's a minimizing or maximizing player
            if (board.IsWhiteToMove)
            {
                bestEval = Math.Max(bestEval, moveEval);
                alpha = Math.Min(alpha, bestEval);
            }
            else
            {
                bestEval = Math.Min(bestEval, moveEval);
                beta = Math.Max(beta, bestEval);
            }
            if (beta <= alpha)
            {
                return bestEval;
            }
            if (depth == searchDepth && moveEval == bestEval)
            {
                bestMove = move;
            }
        }
        
        return bestEval;
    }

    private int EvaluateBoard()
    {
        int materialValue = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();

        if (board.IsDraw())
        {
            return 0;
        }
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? int.MaxValue : int.MinValue;
        }
        // Loop through each piece type and add the difference in material value to the total
        for(int i = 0;i < 5; i++){
            materialValue += (pieceLists[i].Count - pieceLists[i + 6].Count) * pointValues[i];
        }
        return materialValue;
    }
}