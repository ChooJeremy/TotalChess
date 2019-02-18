using System.Collections.Generic;
using UnityEngine;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    Board board = new Board(16, 16);
    HashSet<Square> lockedSquares = new HashSet<Square>();
    struct MoveMetaData
    {
        public Move move;
        public Square nextSquare;
        public bool wasMoveApplied;
        public bool isBounce;

        public MoveMetaData(Move move, Square nextSquare, bool moveApplied = false, bool isBounce = false)
        {
            this.move = move;
            this.nextSquare = nextSquare;
            this.wasMoveApplied = moveApplied;
            this.isBounce = isBounce;
        }
    }

    void ResetLockedSquares()
    {
        lockedSquares = new HashSet<Square>();
    }

    void LockSquares(IEnumerable<Square> squares)
    {
        foreach (Square square in squares)
            LockSquare(square);
    }

    void LockSquare(Square square)
    {
        lockedSquares.Add(square);
    }

    bool IsSquareLocked(Square square)
    {
        return lockedSquares.Contains(square);
    }

    void CalculateNextState()
    {
    }

    void ResolveMovement(Move[] moves)
    {
        ResetLockedSquares();
        IEnumerable<Square> squaresWithStationaryPieces =
            moves.Where(move => move.direction == Move.Direction.NONE)
                 .Select(move => move.piece.currentSquare)
                 .ToArray();

        LockSquares(squaresWithStationaryPieces);

        MoveMetaData[] movesToConsider =
            moves.Where(move => move.direction != Move.Direction.NONE)
                 .Select(move => new MoveMetaData(move, board.NextSquare(move)))
                 .ToArray();

        bool hasInvalidMoves = true;

        while (hasInvalidMoves)
        {
            HashSet<int> moveIndicesRemoved = new HashSet<int>();
            for (int i = 0; i < movesToConsider.Length; i++)
            {
                MoveMetaData moveData = movesToConsider[i];
                // Consider entering a locked square
                bool isNextSquareLocked = IsSquareLocked(moveData.nextSquare);
                if (isNextSquareLocked)
                {
                    LockSquare(moveData.move.piece.currentSquare);
                    moveIndicesRemoved.Add(i);
                    continue;
                }

                // Consider all conflicting moves to the next square
                for (int j = i + 1; j < movesToConsider.Length; j++)
                {
                    MoveMetaData otherMoveData = movesToConsider[j];
                    // Consider bounces
                    if (moveData.nextSquare == otherMoveData.nextSquare)
                    {
                        LockSquare(moveData.move.piece.currentSquare);
                        LockSquare(otherMoveData.move.piece.currentSquare);
                        moveData.isBounce = true; // mark as a potential bounce for now
                        otherMoveData.isBounce = true; // mark as a potential bounce for now
                        moveIndicesRemoved.Add(i);
                        moveIndicesRemoved.Add(j);
                    }

                    // Consider Opposing movement
                    if (moveData.nextSquare == otherMoveData.move.piece.currentSquare &&
                        moveData.move.piece.currentSquare == otherMoveData.nextSquare)
                    {
                        LockSquare(moveData.move.piece.currentSquare);
                        LockSquare(otherMoveData.move.piece.currentSquare);
                        moveIndicesRemoved.Add(i);
                        moveIndicesRemoved.Add(j);
                    }
                }
            }

            int numConsideredMove = movesToConsider.Length;
            movesToConsider = movesToConsider
                .Where((moveData, index)=> !moveIndicesRemoved.Contains(index))
                .ToArray();

            hasInvalidMoves = numConsideredMove != movesToConsider.Length;
        }

        // apply valid moves
        MoveMetaData[] validMoves = movesToConsider;
        MoveMetaData[] completedMoves =
            validMoves
                .Select(moveData =>
                {
                    Debug.Assert(!IsSquareLocked(moveData.nextSquare)); // sanity check
                    moveData.wasMoveApplied = true;
                    return moveData;
                })
                .ToArray();

        // find out the real bounces

        // return


    }
}
