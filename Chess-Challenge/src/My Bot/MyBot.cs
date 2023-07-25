using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int maxDepth = 4;
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    int[] pointValues = {100, 350, 350, 525, 1000, 99999};
    public Move Think(Board board, Timer timer)
    {
        this.board = board;

        // Call the Minimax algorithm to find the best move
        Console.WriteLine(Search(maxDepth, -30000, 30000, board.IsWhiteToMove ? 1 : -1) + "  " + bestMove + " is white turn: " + board.IsWhiteToMove);
        return bestMove;
    }

    // Negamax algorithm with alpha-beta pruning
    private int Search(int depth, int alpha, int beta, int color)
    {
        // If the search reaches the desired depth or the end of the game, evaluate the position and return its value
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsDraw()) return 0;
            
            if (board.IsInCheckmate()) return -30000 + (maxDepth - depth);
            
            return EvaluateBoard(color);
        }
        Move[] legalMoves = board.GetLegalMoves();
        int bestEval = -30000;
        int eval;
        // Generate and loop through all legal moves for the current player
        for (int i = 0; legalMoves.Length > i; i++)
        {
            Move move = legalMoves[i];
            // Make the move on a temporary board and call search recursively
            board.MakeMove(move);
            eval = -Search(depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);

            // Update the best move and prune if necessary
            if (eval > bestEval)
            {
                bestEval = eval;
                if (depth == maxDepth) bestMove = move;
                // Improve alpha
                alpha = Math.Max(alpha, eval);
                
                if (alpha >= beta) break;
                
            }
        }
        
        return bestEval;
    }

    private int EvaluateBoard(int color)
    {
        int materialValue = 0;
        int mobilityValue = board.GetLegalMoves().Length;
        PieceList[] pieceLists = board.GetAllPieceLists();
        // Loop through each piece type and add the difference in material value to the total
        for(int i = 0;i < 5; i++){
            materialValue += (pieceLists[i].Count - pieceLists[i + 6].Count) * pointValues[i];
        }
        return materialValue * color + mobilityValue * color;
    }
}