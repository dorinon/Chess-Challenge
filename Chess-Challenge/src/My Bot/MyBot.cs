using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int maxDepth = 4;
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    int[] piecesValues = {100, 350, 350, 525, 1000, 99999};
    public Move Think(Board board, Timer timer)
    {
        this.board = board;

        // Call the Minimax algorithm to find the best move
        int eval = Search(maxDepth, -30000, 30000, board.IsWhiteToMove ? 1 : -1);
        Console.WriteLine(eval + "  " + bestMove + " is white turn: " + board.IsWhiteToMove);
        return bestMove;
    }

    // Negamax algorithm with alpha-beta pruning
    private int Search(int depth, int alpha, int beta, int color)
    {
        
        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition() || board.FiftyMoveCounter >= 100) return 0;                                      
        if (board.GetLegalMoves().Length == 0) return board.IsInCheck() ? -29000 - depth : 0;
        bool qsearch = depth <= 0;
        int bestEval = -30000;
        int eval;
        if (qsearch)
        {
            bestEval = EvaluateBoard(color);
            //eval is StandPat
            if(depth <= -4 || bestEval >= beta) return bestEval;
        }
        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        
        for(int i = 0; i < moves.Length; i++) {
            Move move = moves[i];

            if(move.IsCapture) scores[i] = (int)move.CapturePieceType - (int)move.MovePieceType;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            for(int j = i + 1; j < moves.Length; j++) {
                if(scores[j] > scores[i])
                    (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
            }
        }
        // Generate and loop through all legal moves for the current player
        for (int i = 0; moves.Length > i; i++)
        {
            // Incrementally sort moves
            Move move = moves[i];
            // Make the move on a temporary board and call search recursively
            board.MakeMove(move);
            eval = -Search(depth -1, -beta, -alpha, -color);
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
        // Loop through each piece type and add the difference in material value to the total
        for (int i = 0;i < 6; i++)
            materialValue += (board.GetPieceList((PieceType)i + 1, true).Count - board.GetPieceList((PieceType)i + 1, false).Count) * piecesValues[i];
        return materialValue * color + mobilityValue;
    }
}