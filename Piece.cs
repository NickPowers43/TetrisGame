using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenGLLib;
using OpenGLLib.TetrisStuff;
using OpenGLLib.TetrisStuff.UniformColor;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TetrisGame
{
    class Piece
    {
        public const int
            SIZE = 4,
            ORIGIN_OFFSET = 2;

        private static bool[][] PIECE_TEMPLATES = GeneratePieces();

        private Vector3 color;
        private bool[] state;
        private int x, y;
        private Pieces type;

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        public bool[] State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }
        public Vector3 Color
        {
            get
            {
                return color;
            }
        }

        public Piece(Pieces type, int x, int y, Vector3 color)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.color = color;
            this.state = CopyData(PIECE_TEMPLATES[(int)type]);
        }

        public bool IsOverlapping(Board board)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    if (board.GetStateValue(BoardX(x), BoardY(y)).occupied & GetStateValue(x, y))
                        return true;
                }
            }

            return false;
        }
        public bool IsCollidingWithFloor()
        {
            int boardY = BoardY(0);

            if (boardY >= 0)
                return false;

            int last = -boardY;

            for (int y = 0; y < last; y++)
            {
                if (ScanRow(y))
                    return true;
            }

            return false;
        }

        private bool OutsideBoard()
        {
            if (this.x < ORIGIN_OFFSET)
                return true;
            else if (this.x > Board.WIDTH - SIZE + ORIGIN_OFFSET)
                return true;
            else if (this.y < ORIGIN_OFFSET)
                return true;
            else if (this.y > Board.HEIGHT - SIZE + ORIGIN_OFFSET)
                return true;
            else
                return false;
        }
        public int BoardX(int localX)
        {
            return this.x - ORIGIN_OFFSET + localX;
        }
        public int BoardY(int localY)
        {
            return this.y - ORIGIN_OFFSET + localY;
        }
        private bool ScanColumn(int column)
        {
            for (int y = 0; y < SIZE; y++)
            {
                if (GetStateValue(column, y))
                    return true;
            }

            return false;
        }
        private bool ScanRow(int row)
        {
            for (int x = 0; x < SIZE; x++)
            {
                if (GetStateValue(x, row))
                    return true;
            }

            return false;
        }
        public bool GetStateValue(int x, int y)
        {
            return state[(y * SIZE) + x];
        }
        public bool GetOldStateValue(bool[] oldState, int x, int y)
        {
            return oldState[(y * SIZE) + x];
        }
        public bool SetStateValueToTrue(int x, int y)
        {
            return state[(y * SIZE) + x] = true;
        }
        public void CollideWithSides()
        {
            int offset;

            if (OutsideBoard())
            {

            }

            if (this.x < Piece.ORIGIN_OFFSET)
            {
                offset = -(this.x - Piece.ORIGIN_OFFSET);
                //colliding with left wall
                for (int x = 0; x < offset; x++)
                {
                    if (ScanColumn(x))
                    {
                        this.x++;
                        break;
                    }
                }
            }
            else if (this.x > Board.WIDTH - 2)
            {
                offset = Board.WIDTH - (this.x - Piece.ORIGIN_OFFSET);
                //colliding with right wall
                for (int x = SIZE - 1; x >= offset; x--)
                {
                    if (ScanColumn(x))
                    {
                        this.x--;
                        break;
                    }
                }
            }
        }
        public void SetInPlace(Board board)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    if (GetStateValue(x, y))
                    {
                        int
                            tempX = BoardX(x),
                            tempY = BoardY(y);
                        board.SetStateValue(tempX, tempY, new Block(color, true));
                    }
                }
            }
        }
        static bool[][] GeneratePieces()
        {
            bool[][] output = new bool[7][];

            output[0] = new bool[16] { false, false, false, false, false, true, true, false, false, true, true, false, false, false, false, false };
            output[1] = new bool[16] { false, false, false, false, false, false, false, false, true, true, true, true, false, false, false, false };
            output[2] = new bool[16] { false, false, false, false, false, true, true, false, false, false, true, true, false, false, false, false };
            output[3] = new bool[16] { false, false, false, false, false, false, true, true, false, true, true, false, false, false, false, false };
            output[4] = new bool[16] { false, false, false, false, false, true, false, false, false, true, true, true, false, false, false, false };
            output[5] = new bool[16] { false, false, false, false, false, false, false, true, false, true, true, true, false, false, false, false };
            output[6] = new bool[16] { false, false, false, false, false, false, true, false, false, true, true, true, false, false, false, false };

            return output;
        }
        bool[] CopyData(bool[] input)
        {
            bool[] copy = new bool[16];

            for (int i = 0; i < 16; i++)
                copy[i] = input[i];

            return copy;
        }
        public void RotateCW()
        {
            if (type == Pieces.Bar & state[2])
            {
                RotateCC();
                return;
            }

            bool[] oldState = state;
            state = new bool[SIZE * SIZE];

            int offx, offy;

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    int
                        setX = x - ORIGIN_OFFSET,
                        setY = y - ORIGIN_OFFSET;

                    int old =  setY;

                    setY = setX;
                    setX = -old;

                    setX += ORIGIN_OFFSET;
                    setY += ORIGIN_OFFSET;

                    if (GetOldStateValue(oldState, x, y))
                        SetStateValueToTrue(setX, setY);
                }
            }
        }
        public void RotateCC()
        {
            if (type == Pieces.Bar & state[8])
            {
                RotateCW();
                return;
            }

            bool[] oldState = state;
            state = new bool[SIZE * SIZE];

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    int
                        setX = x - ORIGIN_OFFSET,
                        setY = y - ORIGIN_OFFSET;

                    int old = setX;

                    setX = setY;
                    setY = -old;

                    setX += ORIGIN_OFFSET;
                    setY += ORIGIN_OFFSET;

                    if (GetOldStateValue(oldState, x, y))
                        SetStateValueToTrue(setX, setY);
                }
            }
        }
    }
}
