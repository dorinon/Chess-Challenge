using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int maxDepth = 3;
    private Move bestMove;
    private Board board;
    private int bigNum = 1000000;
    // Point values for each piece type for evaluation
    int[] pointValues = {100, 350, 350, 525, 1000, 99999};
    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        
        // Call the Minimax algorithm to find the best move
        Console.WriteLine(Minimax(maxDepth, -bigNum, bigNum, board.IsWhiteToMove ? 1 : -1) + "  " + bestMove);
        return bestMove;
    }

    // Minimax algorithm with alpha-beta pruning
    private int Minimax(int depth, int alpha, int beta, int color)
    {
        // If the search reaches the desired depth or the end of the game, evaluate the position and return its value
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsDraw())
            {
                return 0;
            }
            if (board.IsInCheckmate())
            {
                return (bigNum - (maxDepth - depth)) * -color;
            }
            return EvaluateBoard(color);
        }
        int bestEval = -bigNum * color;
        int moveEval = bestEval;

        // Generate and loop through all legal moves for the current player
        foreach (Move move in board.GetLegalMoves())
        {
            // Make the move on a temporary board and call Minimax recursively
            board.MakeMove(move);

            moveEval = Minimax(depth - 1, -beta, -alpha, -color);
            
            board.UndoMove(move);
            /*alpha = Math.Max(alpha, moveEval);
            if (alpha <= beta)
            {
                break;
            }*/
            // Update the best evaluation and alpha/beta values based on whether it's a minimizing or maximizing player
            if (moveEval * color > bestEval * color)
            {
                bestEval = moveEval;
                if (depth == 3) bestMove = move;
            }
        }
        
        return bestEval;
    }

    private int EvaluateBoard(int color)
    {
        int materialValue = 0;
        int mobilityValue = board.GetLegalMoves().Length * color;
        PieceList[] pieceLists = board.GetAllPieceLists();
        // Loop through each piece type and add the difference in material value to the total
        for(int i = 0;i < 5; i++){
            materialValue += (pieceLists[i].Count - pieceLists[i + 6].Count) * pointValues[i];
        }
        return materialValue + mobilityValue;
    }
}