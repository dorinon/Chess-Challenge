using System;
using System.Linq;
using System.Numerics;
using System.Text;
using ChessChallenge.API;
using Microsoft.VisualBasic.CompilerServices;

public class MyBot : IChessBot
{
    private Move bestMove;
    private Board board;
    // Point values for each piece type for evaluation
    
    //removed the minimum ps to stabilize with pst
     short[] PieceValues = {65, 183, 268, 454, 950, 29935, // Middlegame
                            92, 251, 323, 505, 957, 29926}; // Endgame

     private short[] gamePhaseInc = {0, 1, 1, 2, 4, 0};
     int[] psts = new int[64 * 12];
     decimal[] quantizedArray = {
        27909335506732266436165655m,  8099897515490484006734673175m,  11813746079450960178425387799m,
        11817358744429988596699516951m,  13364774348659602364205341719m,  18619974868293840544160493335m,
        16446316687411893639780788247m,  12121979510401049595651172631m,  13019021564572452270008320091m,
        19240158592432847116017292915m,  18630860126053506528853074753m,  19257078739588156431296072280m,
        19271538736081412984757060166m,  23576542339272103092960664686m,  20485338018742398357439018799m,
        17985260403821339742106510864m,  17658864803890118367929782803m,  19228050407286765453103438620m,
        20468408391883416066315356969m,  18644134589196265210548297261m,  19880860979860947388257577796m,
        25132411220235313898160048702m,  24809623229745987154340848681m,  18301980319386538006273232906m,
        13964377999423073030539406350m,  20169789657647491978098343200m,  20790001697503450008945786652m,
        21427067788613308634678072870m,  21127258981501210608324938535m,  22660172142562572502423413024m,
        21127249388549445197676445219m,  16158573694922487778296365320m,  11781053209863340821648271621m,
        14603904554652582685970625814m,  20167390751457873873990679828m,  20809316268792260891178007328m,
        21414959622213990359476700451m,  20488917758988248378494713628m,  17398898475019536371617267742m,
        13362285643724625975990447110m,  11472753440024384478058668550m,  14867431420219487971753225237m,
        17997326329371099189272017429m,  20156482047989786994519734545m,  20468361334222924360815050522m,
        18928175543157876813111917850m,  17065234892311914892331615022m,  13656035654420793123076466447m,
        9920487927444181762554093056m,  13323614073845617228270161431m,  16412438561986469389087762954m,
        18281447397919378546827489288m,  18590894702740531601196478733m,  16418454838005141720453774888m,
        14550659650403265407907424305m,  12078429812467256652092433672m,  4340076789263538792515578391m,
        8368255582537961417760727831m,  11158456191182218035179964951m,  13307917093891974474926480151m,
        9625514824545891310680106519m,  12697366905970192825153707799m,  10540700058164212799285060887m,
        6500394244407116926542242327m
    };
    public MyBot() {
        for (int i = 0; i < 64 * 12; i++) psts[i] = (int)((int)(((BigInteger)quantizedArray[i / 12] >> (i % 12 * 8)) & 255) * 1.461f);
    }
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
    const int Entries = 2^22 - 3;
    // Transposition table
    TTEntry[] _tt = new TTEntry[Entries];
    
    // Negamax algorithm with alpha-beta pruning
    private int Search(int depth, int alpha, int beta, int color, int ply, Timer timer)
    {
        ulong key = board.ZobristKey;
        bool qsearch = depth <= 0, notRoot = ply > 0;
        int bestEval = -30000, eval, origAlpha = alpha;
        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        
        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition()) return 50;                                      
        if (board.GetLegalMoves().Length == 0) return board.IsInCheck() ? -29000 + ply : 0;
        
        TTEntry entry = _tt[key % Entries];
        if (notRoot && entry.key == key && (entry.bound == 3 || entry.bound == 2 && entry.score >= beta || entry.bound == 1 && entry.score <= alpha))
        {
            usedTT++;
            if (entry.depth >= depth) return entry.score;
        }
        
        if (qsearch)
        {
            bestEval = Evaluate(color);
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
        // Generate and loop through all legal moves for the current player
        for (int i = moves.Length - 1; -1 < i; i--)
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
        _tt[key % Entries] = new TTEntry(key, depth, bestEval, bound, bestMove.RawValue);
        
        return bestEval;
    }
    private int Evaluate(int color)
    {
        int[] middleGame = new int[2], endGame = new int[2];
        int gamePhase = 0, materialValue, mobilityValue = board.GetLegalMoves().Length;
        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i < 12; i++)
        {
            int pieceType = i % 6;
            int colour = i / 6;
            middleGame[colour] += pieceLists[i].Count() * PieceValues[pieceType];
            endGame[colour] += pieceLists[i].Count() * PieceValues[pieceType + 6];
            gamePhase += pieceLists[i].Count() * gamePhaseInc[pieceType];
            for (int j = 0; j < pieceLists[i].Count; j++)
            {
                int square = pieceLists[i][j].Square.Index;
                middleGame[colour] +=  psts[12 * square + pieceType];
                endGame[colour] += psts[12 * square + pieceType + 6];
            }
        }
        materialValue = ((middleGame[0] - middleGame[1]) * Math.Min(gamePhase, 24) + (endGame[0] - endGame[1]) * (24 - gamePhase)) / 24;

        return materialValue * color + mobilityValue;
    }
    public Move Think(Board board, Timer timer)
     {
         this.board = board;
         int preBestScore = -999999;
         usedTT = 0;
         int score;
         int depth = 0;
         // Iterative deepening loop
         for (int i = 1; i < 50; i++)
         {
             score = Search(i, -30000, 30000, board.IsWhiteToMove ? 1 : -1, 0, timer);
             depth = i;
             if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) break;
             preBestScore = score;
         }
         // Call the Minimax algorithm to find the best move
         Console.WriteLine(bestMove + "  " + preBestScore + " is white turn: " + board.IsWhiteToMove + " depth reached: " + depth + " " + " number of TT entries used: " + usedTT);
         return bestMove;
     }
}