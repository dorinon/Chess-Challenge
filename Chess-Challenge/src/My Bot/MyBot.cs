using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int maxDepth = 4;
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    int[] piecesValues = {100, 350, 350, 525, 1000, 99999};
    int usedTT;
    
    // Transposition table entry
    struct TTEntry {
        public ulong key;
        public Move move;
        public int depth, score, bound;
        public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
            key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        }
    }
    const int entries = 1<<20;
    // Transposition table
    TTEntry[] tt = new TTEntry[entries];



    // Negamax algorithm with alpha-beta pruning
    private int Search(int depth, int alpha, int beta, int color)
    {
        ulong key = board.ZobristKey;
        
        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition() || board.FiftyMoveCounter >= 100) return 50;                                      
        if (board.GetLegalMoves().Length == 0) return board.IsInCheck() ? -29000 - depth : 0;
        
        TTEntry entry = tt[key % entries];
        if ((depth != maxDepth && entry.key == key && entry.depth >= depth) &&
            (entry.bound == 3 || entry.bound == 2 && entry.score >= beta ||
             entry.bound == 1 && entry.score <= alpha)
           )
        {
            usedTT++;
            return entry.score;
        }

        
        bool qsearch = depth <= 0;
        int bestEval = -30000;
        int eval;
        int origAlpha = alpha;
        if (qsearch)
        {
            bestEval = EvaluateBoard(color);
            //eval is StandPat
            if(bestEval >= beta) return bestEval;
        }
        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        
        for(int i = 0; i < moves.Length; i++) {
            Move move = moves[i];

            if(move.IsCapture) scores[i] = (int)move.CapturePieceType - (int)move.MovePieceType;
        }
        
        // Generate and loop through all legal moves for the current player
        for (int i = 0; moves.Length > i; i++)
        {
            // Incrementally sort moves
            for(int j = i + 1; j < moves.Length; j++) {
                if(scores[j] > scores[i])
                    (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
            }
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
        // Did we fail high/low or get an exact score?
        int bound = bestEval >= beta ? 2 : bestEval > origAlpha ? 3 : 1;

        // Push to TT
        tt[key % entries] = new TTEntry(key, bestMove, depth, bestEval, bound);
        
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
    public Move Think(Board board, Timer timer)
     {
         this.board = board;
         usedTT = 0;
         // Call the Minimax algorithm to find the best move
         int eval = Search(maxDepth, -30000, 30000, board.IsWhiteToMove ? 1 : -1);
         Console.WriteLine(eval + "  " + bestMove + " is white turn: " + board.IsWhiteToMove + " number of TT entries used: " + usedTT);
         return bestMove;
     }
}