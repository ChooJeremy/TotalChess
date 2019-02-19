using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace State {

    enum Player { A, B }

    struct Piece
    {
        public enum Type { SWORD, SPEAR, HORSE }
        public Player owner;
        public int health;
        public int attack;
        public int def;
        public Type type;

        public string uid; // unique id for a piece
        public Square currentSquare;

        public Piece(
            string uid,
            Player owner,
            Square square,
            Type type = Type.SWORD,
            int health = 100,
            int attack = 5,
            int def = 5
        )
        {
            this.uid = uid;
            this.owner = owner;
            this.currentSquare = square;
            this.health = health;
            this.attack = attack;
            this.def = def;
            this.type = type;
        }
    }

    struct Move
    {
        public enum Direction { UP, DOWN, LEFT, RIGHT, NONE }
        public Piece piece;
        public Direction direction;

        public Move(Piece piece, Direction direction = Move.Direction.NONE)
        {
            this.piece = piece;
            this.direction = direction;
        }
    }

    struct Square
    {
        public int row;
        public int col;
        public Square(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public override int GetHashCode()
        {
            return row * 31 + col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this == (Square) obj;
        }

        public static bool operator ==(Square a, Square b)
        {
            return a.row == b.row && a.col == b.col;
        }

        public static bool operator !=(Square a, Square b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[Square row:{0} col:{1}]", row, col);
        }
    }

    class Board
    {
        int numRows;
        int numCols;
        List<Piece> pieces;

        public Board(int rows, int cols)
        {
            numRows = rows;
            numCols = cols;
        }

        public Square NextSquare(Square currentSquare, Move.Direction direction)
        {
            int row, col;
            switch (direction)
            {
                case Move.Direction.UP:
                    row = currentSquare.row > 0 ? currentSquare.row - 1 : 0;
                    return new Square(row, currentSquare.col);
                case Move.Direction.DOWN:
                    row = currentSquare.row < numRows -1 ? currentSquare.row + 1 : numRows - 1;
                    return new Square(row, currentSquare.col);
                case Move.Direction.LEFT:
                    col = currentSquare.col > 0 ? currentSquare.col - 1 : 0;
                    return new Square(currentSquare.row, col);
                case Move.Direction.RIGHT:
                    col = currentSquare.col < numCols - 1 ? currentSquare.col + 1 : numCols - 1;
                    return new Square(currentSquare.row, col);
            }
            return currentSquare;
        }

        public Square NextSquare(Move move)
        {
            Square currentSquare = move.piece.currentSquare;
            Move.Direction direction = move.direction;
            return NextSquare(currentSquare, direction);
        }
    }

}
