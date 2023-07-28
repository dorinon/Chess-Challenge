using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    int[] piecesValues = {100, 350, 350, 525, 1000, 99999};
    int usedTT;
    
    // Transposition table entry
    struct TTEntry {
        public ulong key;
        public ushort bestMove;
        public int depth, score, bound;
        public TTEntry(ulong _key, int _depth, int _score, int _bound, ushort _bestMove) {
            key = _key; depth = _depth; score = _score; bound = _bound; bestMove = _bestMove;
        }
    }
    const int entries = 2^22 - 3;
    // Transposition table
    TTEntry[] tt = new TTEntry[entries];
    
    // Negamax algorithm with alpha-beta pruning
    private int Search(int depth, int alpha, int beta, int color, int ply, Timer timer)
    {
        ulong key = board.ZobristKey;
        bool qsearch = depth <= 0;
        bool notRoot = ply > 0;
        int bestEval = -30000;
        int eval;
        int origAlpha = alpha;
        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        
        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition() || board.FiftyMoveCounter >= 100) return 50;                                      
        if (board.GetLegalMoves().Length == 0) return board.IsInCheck() ? -29000 - depth : 0;
        
        TTEntry entry = tt[key % entries];
        if (notRoot && entry.key == key && (entry.bound == 3 || entry.bound == 2 && entry.score >= beta || entry.bound == 1 && entry.score <= alpha))
        {
            if (entry.depth >= depth) return entry.score;
            usedTT++;
        }
        
        if (qsearch)
        {
            bestEval = EvaluateBoard(color);
            //eval is StandPat
            if(bestEval >= beta || depth < -10) return bestEval;
        }
        
        for(int i = 0; i < moves.Length; i++) {
            Move move = moves[i];
            if(move.IsCapture) scores[i] = 20 * (int)move.CapturePieceType - (int)move.MovePieceType;
            if (move.RawValue == entry.bestMove)
            {
                scores[i] = 1000000;
            }
        }
        Array.Sort(scores, moves);
        Array.Reverse(moves);
        // Generate and loop through all legal moves for the current player
        for (int i = 0; moves.Length > i; i++)
        {
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 30000;
            Move move = moves[i];
            // Make the move on a temporary board and call search recursively
            board.MakeMove(move);
            eval = -Search(depth -1, -beta, -alpha, -color, ply+1, timer);
            board.UndoMove(move);

            // Update the best move and prune if necessary
            if (eval > bestEval)   
            {
                bestEval = eval;
                if (!notRoot) bestMove = move;
                
                // Improve alpha
                alpha = Math.Max(alpha, eval);
                
                if (alpha >= beta) break;
            }
            
        }
        // Did we fail high/low or get an exact score?
        int bound = bestEval >= beta ? 2 : bestEval > origAlpha ? 3 : 1;

        // Push to TT
        tt[key % entries] = new TTEntry(key, depth, bestEval, bound, bestMove.RawValue);
        
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
         Move preBestMove = Move.NullMove;
         int preBestScore = -999999;
         usedTT = 0;
         int score;
         //TODO: find way to salvage last search result if it is interrupted
         for (int i = 1; i < 50; i++)
         {
             score = Search(i, -30000, 30000, board.IsWhiteToMove ? 1 : -1, 0, timer);
             
             if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) break;
             preBestMove = bestMove;
             preBestScore = score;
         }
         // Call the Minimax algorithm to find the best move
         Console.WriteLine(preBestMove + "  " + preBestScore + " is white turn: " + board.IsWhiteToMove + " number of TT entries used: " + usedTT);
         return preBestMove;
     }
}