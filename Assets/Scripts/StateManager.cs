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
        public bool isStationary;

        public MoveMetaData(Move move, Square nextSquare, bool moveApplied = false, bool isBounce = false, bool stationary = false)
        {
            this.move = move;
            this.nextSquare = nextSquare;
            this.wasMoveApplied = moveApplied;
            this.isBounce = isBounce;
            this.isStationary = stationary;
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

    MoveMetaData[] ResolveMovement(Move[] moves)
    {
        ResetLockedSquares();

        MoveMetaData[] moveMetaDatas =
            moves.Select(move => new MoveMetaData(move, board.NextSquare(move)))
                 .ToArray();

        List<int> moveIndices = Enumerable.Range(0, moves.Length).ToList();

        // Consider stationary pieces
        moveIndices = moveIndices.Where(mInd =>
        {
            MoveMetaData moveData = moveMetaDatas[mInd];
            bool isStationary = moveData.move.direction == Move.Direction.NONE;
            if (isStationary) LockSquare(moveData.move.piece.currentSquare);
            moveData.isStationary = isStationary;
            return !isStationary;
        }).ToList();

        bool hasInvalidMoves = true;

        while (hasInvalidMoves)
        {
            int previousLength = moveIndices.Count;
            moveIndices = moveIndices.Where((mInd, index) =>
            {
                MoveMetaData moveData = moveMetaDatas[mInd];
                bool isValidMove = true;
                //Consider entering a locked square
                if (IsSquareLocked(moveData.nextSquare))
                {
                    LockSquare(moveData.move.piece.currentSquare);
                    isValidMove = false;
                }

                //Consider interactions with other pieces
                for (int i = index + 1; i < moveIndices.Count; i++)
                {
                    int otherIndex = moveIndices[i]; // other move index for consideration
                    MoveMetaData otherMoveData = moveMetaDatas[otherIndex];
                    if (moveData.nextSquare == otherMoveData.nextSquare)
                    {
                        LockSquare(moveData.move.piece.currentSquare);
                        LockSquare(otherMoveData.move.piece.currentSquare);
                        moveData.isBounce = true; // mark as a potential bounce for now
                        otherMoveData.isBounce = true; // mark as a potential bounce for now
                        isValidMove = false;
                    }

                    // Consider Opposing movement
                    if (moveData.nextSquare == otherMoveData.move.piece.currentSquare &&
                       moveData.move.piece.currentSquare == otherMoveData.nextSquare)
                    {
                        LockSquare(moveData.move.piece.currentSquare);
                        LockSquare(otherMoveData.move.piece.currentSquare);
                        isValidMove = false;
                    }

                }
                return isValidMove;
            }).ToList();
            hasInvalidMoves = moveIndices.Count != previousLength;
        }

        // apply valid moves
        List<int> validMoveIndices = moveIndices;
        validMoveIndices.ForEach(validMoveIndex =>
        {
            MoveMetaData moveData = moveMetaDatas[validMoveIndex];
            Debug.Assert(!IsSquareLocked(moveData.nextSquare)); // sanity check
            Debug.Assert(!moveData.isStationary); // sanity check
            Debug.Assert(!moveData.isBounce); // sanity check

            moveData.wasMoveApplied = true;
        });

        // find out the real bounces
        for (int i = 0; i < moveMetaDatas.Length; i++)
        {
            MoveMetaData moveData = moveMetaDatas[i];
            if (!moveData.isBounce) continue;
            moveData.isBounce = !IsSquareLocked(moveData.nextSquare);
        }

        // return
        return moveMetaDatas;
    }
}
