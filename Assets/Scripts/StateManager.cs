using System.Collections.Generic;
using UnityEngine;
using State;
using System;
using System.Linq;

public class StateManager : MonoBehaviour {
    HashSet<Square> lockedSquares = new HashSet<Square>();

    void Start()
    {
        CalculateNextState();
    }

    class MoveMetaData
    {
        public Move move;
        public Square nextSquare;
        public bool shouldMove;
        public bool isBounce;
        public bool isStationary;

        public MoveMetaData(Move move, Square nextSquare, bool shouldMove = false, bool isBounce = false, bool stationary = false)
        {
            this.move = move;
            this.nextSquare = nextSquare;
            this.shouldMove = shouldMove;
            this.isBounce = isBounce;
            this.isStationary = stationary;
        }

        public override string ToString()
        {
            return String.Format(
                "[Move Piece {0}, currentSq {1}, nextSq {2}, shouldMove {3}, isBounce {4}, isStationary {5}]",
                move.piece.uid,
                move.piece.currentSquare,
                nextSquare,
                shouldMove,
                isBounce,
                isStationary
            );
        }
    }

    // FOR TESTING:

    Board board = new Board(6, 6);                                     //-------------------------------
    Piece A_PIECE_1 = new Piece("uida1", Player.A, new Square(0, 0));  //| A1 |    |    |    |    |    |
    Piece A_PIECE_2 = new Piece("uida2", Player.A, new Square(2, 2));  //|    |    |    | A3 |    |    |
    Piece A_PIECE_3 = new Piece("uida3", Player.A, new Square(1, 3));  //|    |    | A2 |    |    |    |
                                                                       //|    |    | B1 | B2 | B3 |    |
    Piece B_PIECE_1 = new Piece("uidb1", Player.B, new Square(3, 2));  //|    |    |    |    |    |    |
    Piece B_PIECE_2 = new Piece("uidb2", Player.B, new Square(3, 3));  //|    |    |    |    |    |    |
    Piece B_PIECE_3 = new Piece("uidb3", Player.B, new Square(3, 4));  //-------------------------------

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
        Move[] moves = new Move[]
        {
            new Move(A_PIECE_1),
            new Move(A_PIECE_2, Move.Direction.DOWN),
            new Move(A_PIECE_3, Move.Direction.DOWN),
            new Move(B_PIECE_1, Move.Direction.UP),
            new Move(B_PIECE_2, Move.Direction.UP),
            new Move(B_PIECE_3, Move.Direction.LEFT),
        };
        MoveMetaData[] resolvedMoveDatas = ResolveMovement(moves);
        foreach (MoveMetaData resolveData in resolvedMoveDatas)
        {
            Debug.Log(resolveData);
        }
        foreach (Square s in lockedSquares)
        {
            Debug.Log(s);
        }
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
            bool isStationary =
                moveData.move.direction == Move.Direction.NONE ||
                moveData.move.piece.currentSquare == moveData.nextSquare;
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
                bool isValidMove = !moveData.isBounce;
                //Consider entering a locked square
                if (IsSquareLocked(moveData.nextSquare))
                {
                    LockSquare(moveData.move.piece.currentSquare);
                    isValidMove = false;
                }

                //Consider interactions with other pieces
                for (int i = 0; i < moveIndices.Count; i++) // can optimize
                {
                    if (i == index) continue;
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
            moveData.shouldMove = true;
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
