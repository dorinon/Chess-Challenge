using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move moveToPlay = moves[0];
        int highestValueMove = 0;
        
        return moveToPlay;
        
    }

    Move evalPos(Board board){
        int depth = 3;
        int matrialValue = 0;
        int[] pointValues = {100, 350, 350, 525, 1000, 10000};

        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];
        int bestMoveValue = 0;
        foreach(Move move in legalMoves){
            for(int y = 0; y < depth; y++){
                matrialValue = 0;
                for(int i = 0;i < 5; i++){
                    matrialValue += (board.GetAllPieceLists()[i].Count - board.GetAllPieceLists()[i + 6].Count) * pointValues[i];
                }
            }
        }
        return bestMove;
    }
}