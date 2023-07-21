using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move moveToPlay = moves[0];
        int highestValueMove = 0;
        foreach(Move move in moves){
            board.MakeMove(move);
            if(evalPos(board) > highestValueMove){
                highestValueMove = evalPos(board);
                moveToPlay = move;
            }
            board.UndoMove(move);
        }
        return moveToPlay;
        
    }

    int evalPos(Board board){
        int matrialValue = 0;
        int[] pointValues = {100, 350, 350, 525, 1000, 10000};
        for(int i = 0;i < 5; i++){
            matrialValue += (board.GetAllPieceLists()[i].Count - board.GetAllPieceLists()[i + 6].Count) * pointValues[i];
        }
        return matrialValue;
    }
}