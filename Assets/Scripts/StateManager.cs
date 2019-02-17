using System.Collections.Generic;
using UnityEngine;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    Board board = new Board(16, 16);
    HashSet<Square> lockedSquares = new HashSet<Square>();

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
        Square[] squaresWithStationaryPieces =
            moves.Where(move => move.direction == Move.Direction.NONE)
                 .Select(move => move.piece.currentSquare)
                 .ToArray();

        LockSquares(squaresWithStationaryPieces);

        Tuple<Move, Square>[] movesToConsider =
            moves.Where(move => move.direction != Move.Direction.NONE)
                 .Select(move => Tuple.Create(move, board.NextSquare(move)))
                 .ToArray();

        HashSet<Move> potentialBounces = new HashSet<Move>();
        HashSet<Move> movesRemoved = new HashSet<Move>();
        bool hasInvalidMoves = true;

        while (hasInvalidMoves)
        {

            for (int i = 0; i < movesToConsider.Length; i++)
            {
                Tuple<Move, Square> moveAndNextSquare = movesToConsider[i];
                Move move = moveAndNextSquare.Item1;
                Square nextSquare = moveAndNextSquare.Item2;

                // Consider entering a locked square
                bool isNextSquareLocked = IsSquareLocked(nextSquare);
                if (isNextSquareLocked)
                {
                    LockSquare(move.piece.currentSquare);
                    movesRemoved.Add(move);
                    continue;
                }

                // Consider all conflicting moves to the next square
                for (int j = i + 1; j < movesToConsider.Length; j++)
                {
                    Tuple<Move, Square> otherMoveAndNextSquare = movesToConsider[i];
                    Move otherMove = otherMoveAndNextSquare.Item1;
                    Square otherNextSquare = otherMoveAndNextSquare.Item2;

                    // Consider bounces
                    if (nextSquare == otherNextSquare)
                    {
                        LockSquare(move.piece.currentSquare);
                        LockSquare(otherMove.piece.currentSquare);
                        potentialBounces.Add(move);
                        potentialBounces.Add(otherMove);
                        movesRemoved.Add(move);
                        movesRemoved.Add(otherMove);
                    }

                    // Consider Opposing movement
                    if (nextSquare == otherMove.piece.currentSquare &&
                        move.piece.currentSquare == otherNextSquare)
                    {
                        LockSquare(move.piece.currentSquare);
                        LockSquare(otherMove.piece.currentSquare);
                        movesRemoved.Add(move);
                        movesRemoved.Add(otherMove);
                    }
                }
            }

            int numConsideredMove = movesToConsider.Length;
            movesToConsider = movesToConsider
                .Where(moveAndNextMove => !movesRemoved.Contains(moveAndNextMove.Item1)).ToArray();

            hasInvalidMoves = numConsideredMove != movesToConsider.Length;
        }

        // apply valid moves

        // find out the real bounces

        // return


    }

    void LockSquares(Square[] square)
    {

    }

    void IsInvalidMove()
    {

    }


    void ResolveCombat()
    {

    }
}
