using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        evalPos(board);
        return moves[0];
    }

    float evalPos(Board board){
        int matrialValue = 0;
        int[] pointValues = {100, 350, 350, 525, 1000, 10000};
        for(int i = 0;i < 5; i++){
            matrialValue += (board.GetAllPieceLists()[i].Count - board.GetAllPieceLists()[i + 6].Count) * pointValues[i];
        }
        return matrialValue;
    }
}