using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
    class Board
    {
        private static Vector3[] PIECE_COLORS = GenerateColors();

        public const int
            WIDTH = 12,
            HEIGHT = 20,
            WIDTH_HALF = WIDTH >> 1,
            HEIGHT_HALF = HEIGHT >> 1,
            BLOCK_PIXEL_SIZE = 24,
            BOARD_PIXEL_WIDTH = BLOCK_PIXEL_SIZE * WIDTH,
            BOARD_PIXEL_HEIGHT = BLOCK_PIXEL_SIZE * HEIGHT,
            COLOR_COUNT = 6,
            PIECES_COUNT = 7;

        public const float
            ASPECT_RATIO = (float)WIDTH / (float)HEIGHT,
            SCALE = (ASPECT_RATIO < 1) ? /*WIDESCREEN*/ 2.0f / (float)WIDTH : 2.0f / (float)HEIGHT,
            FRAME_TIME = 0.5f,
            INPUT_TIME = 0.1f;

        private Actions prevAction;
        private Rectangle dimensions;
        private Block[] state;
        private Piece piece;
        private Program program;
        private Vector2 bottomLeft;
        private OpenGLLib.IndexedVerticeBuffer<Vertex, ushort> blockVBA;
        private double elapsedTime;
        private Random random;

        public Rectangle Dimensions
        {
            get
            {
                return dimensions;
            }
            set
            {
                dimensions = value;
            }
        }
        public Block[] State
        {
            get
            {
                return state;
            }
        }

        public Board()
        {

            random = new Random();

            state = new Block[WIDTH * HEIGHT];
            ResetBoard();

            program = new Program();
            program.Use();
            program.Scale = SCALE;
            program.Projection = GetProjectionMatrix();

            ushort[] indices = new ushort[20];
            for (int i = 0; i < 20; i++)
                indices[i] = (ushort)i;

            blockVBA = new IndexedVerticeBuffer<Vertex, ushort>(GenerateVertices(), indices);
            blockVBA.Bind();
            blockVBA.EnableVertexAttribArray(0, 2, VertexAttribPointerType.Float, 12, 0);
            blockVBA.EnableVertexAttribArray(1, 1, VertexAttribPointerType.Float, 12, 8);
            blockVBA.UnBind();

            if (ASPECT_RATIO > 1)//widescreen
                bottomLeft = new Vector2((float)-WIDTH_HALF * SCALE, -1.0f);
            else
                bottomLeft = new Vector2(-1.0f, (float)-HEIGHT_HALF * SCALE);

            piece = CreateRandomPiece();
        }

        bool
            previousA = false,
            previousS = false,
            previousD = false,
            previousQ = false,
            previousE = false;
        float
            simulationElapsedTime = 0,
            InputElapsedTime = 0;
        public void Update(KeyboardDevice keyboard, double elapsedTime)
        {
            //this.elapsedTime += elapsedTime;
            this.simulationElapsedTime += (float)elapsedTime;
            this.InputElapsedTime += (float)elapsedTime;

            #region Handle inputs

            if (keyboard[Key.L])
                ResetBoard();

            if (InputElapsedTime > INPUT_TIME)
            {
                if (keyboard[Key.A])
                {
                    piece.X--;
                    prevAction = Actions.Left;
                    HandleCollision();
                }
                if (keyboard[Key.D])
                {
                    piece.X++;
                    prevAction = Actions.Right;
                    HandleCollision();
                }

                InputElapsedTime = 0;
            }

            //if (keyboard[Key.A])
            //{
            //    if (keyboard[Key.A] != previousA)
            //    {
            //        piece.X--;
            //    }
            //    previousA = true;
            //}
            //else
            //    previousA = false;

            //if (keyboard[Key.D])
            //{
            //    if (keyboard[Key.D] != previousD)
            //    {
            //        piece.X++;
            //    }
            //    previousD = true;
            //}
            //else
            //    previousD = false;

            if (keyboard[Key.Q])
            {
                if (keyboard[Key.Q] != previousQ)
                {
                    piece.RotateCC();
                    prevAction = Actions.RotateCC;
                    HandleCollision();
                }
                previousQ = true;
            }
            else
                previousQ = false;

            if (keyboard[Key.E])
            {
                if (keyboard[Key.E] != previousE)
                {
                    piece.RotateCW();
                    prevAction = Actions.RotateCW;
                    HandleCollision();
                }
                previousE = true;
            }
            else
                previousE = false;
            #endregion

            piece.CollideWithSides();

            if (simulationElapsedTime > FRAME_TIME | keyboard[Key.S])
            {
                simulationElapsedTime = 0;
                //increment simulation

                //apply gravity
                piece.Y--;
                prevAction = Actions.Down;
                HandleCollision();

                //check for collision
                //if (piece.IsOverlapping(this) | piece.IsCollidingWithFloor())
                //{
                //    piece.Y++;

                //    //set piece in place
                //    piece.SetInPlace(this);
                //    ScanForCompleteRows();

                //    //create new piece
                //    piece = CreateRandomPiece();
                //}
            }
        }
        public void Draw()
        {
            program.Use();
            GL.Viewport(0, 0, BOARD_PIXEL_WIDTH, BOARD_PIXEL_HEIGHT);
            
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Block temp = GetStateValue(x, y);

                    if (temp.occupied)
                    {
                        float bottom = bottomLeft.Y + (SCALE * y), left = bottomLeft.X + (SCALE * x);

                        program.Color = temp.color;

                        program.Offset = new Vector2(left, bottom);
                        blockVBA.Draw(BeginMode.Quads, 0, 20);
                    }
                }
            }

            program.Color = new Vector3(piece.Color);

            for (int y = 0; y < Piece.SIZE; y++)
            {
                for (int x = 0; x < Piece.SIZE; x++)
                {
                    int tempX = piece.BoardX(x), tempY = piece.BoardX(y);

                    if (piece.GetStateValue(x, y))
                    {
                        float bottom = bottomLeft.Y + (SCALE * piece.BoardY(y)), left = bottomLeft.X + (SCALE * piece.BoardX(x));

                        program.Offset = new Vector2(left, bottom);
                        blockVBA.Draw(BeginMode.Quads, 0, 20);
                    }
                }
            }

            GL.UseProgram(0);
        }
        public Block GetStateValue(int x, int y)
        {
            if (y < 0 | y >= HEIGHT)
                return Block.EMPTY;
            if (x < 0 | x >= WIDTH)
                return Block.EMPTY;
            return state[(y * WIDTH) + x];
        }
        public void SetStateValue(int x, int y, Block data)
        {
            if (y < 0 | y >= HEIGHT)
                return;
            if (x < 0 | x >= WIDTH)
                return;
            state[(y * WIDTH) + x] = data;
        }
        //public void SetStateValue(int x, int y, Block
        private void ResetBoard()
        {
            for (int i = 0; i < state.Length; i++)
                state[i] = Block.EMPTY;
        }
        private Piece CreateRandomPiece()
        {
            int colorID = random.Next(COLOR_COUNT);
            int pieceID = random.Next(PIECES_COUNT);

            return new Piece((Pieces)pieceID, WIDTH_HALF, HEIGHT - 1, PIECE_COLORS[colorID]);
        }
        private Matrix4 GetProjectionMatrix()
        {
            float x = 1, y = 1;
            Matrix4 proj;

            if (ASPECT_RATIO > 1)
            {
                //widescreen
                x = 1.0f / ASPECT_RATIO;

                proj = Matrix4.Scale(x, y, 1);
            }
            else
            {
                y = ASPECT_RATIO;

                proj = Matrix4.Scale(x, y, 1);
            }

            return proj;

            //GL.MatrixMode(MatrixMode.Modelview);
            //Matrix4 view = Matrix4.Identity;
            //GL.LoadMatrix(ref view);

            //GL.MatrixMode(MatrixMode.Projection);
            //proj = Matrix4.Identity;
            //GL.LoadMatrix(ref proj);
        }
        private Vertex[] GenerateVertices()
        {
            float
                brightness0 = 0.6f,
                brightness1 = 0.7f,
                brightness2 = 0.8f,
                brightness3 = 0.9f,
                brightness4 = 1.0f;

            Vector2
                vector0 = new Vector2(0.0f, 0.0f),
                vector1 = new Vector2(0.0f, 1.0f),
                vector2 = new Vector2(1.0f, 1.0f),
                vector3 = new Vector2(1.0f, 0.0f),
                vector4 = new Vector2(0.2f, 0.2f),
                vector5 = new Vector2(0.2f, 0.8f),
                vector6 = new Vector2(0.8f, 0.8f),
                vector7 = new Vector2(0.8f, 0.2f);

            Vertex[] vertices = new Vertex[]{
                new Vertex(vector0, brightness0),
                new Vertex(vector4, brightness0),
                new Vertex(vector7, brightness0),
                new Vertex(vector3, brightness0),

                new Vertex(vector0, brightness1),
                new Vertex(vector1, brightness1),
                new Vertex(vector5, brightness1),
                new Vertex(vector4, brightness1),

                new Vertex(vector4, brightness2),
                new Vertex(vector5, brightness2),
                new Vertex(vector6, brightness2),
                new Vertex(vector7, brightness2),

                new Vertex(vector7, brightness3),
                new Vertex(vector6, brightness3),
                new Vertex(vector2, brightness3),
                new Vertex(vector3, brightness3),

                new Vertex(vector5, brightness4),
                new Vertex(vector1, brightness4),
                new Vertex(vector2, brightness4),
                new Vertex(vector6, brightness4)};

            return vertices;

            //Color4 color0 = Color4.Yellow;
            //Color4 color1 = Color4.Yellow;
            //color1.R *= 0.9f;
            //color1.G *= 0.9f;
            //color1.B *= 0.9f;
            //Color4 color2 = Color4.Yellow;
            //color2.R *= 0.8f;
            //color2.G *= 0.8f;
            //color2.B *= 0.8f;
            //Color4 color3 = Color4.Yellow;
            //color3.R *= 0.7f;
            //color3.G *= 0.7f;
            //color3.B *= 0.7f;
            //Color4 color4 = Color4.Yellow;
            //color4.R *= 0.6f;
            //color4.G *= 0.6f;
            //color4.B *= 0.6f;

            //Vertex[] vertices = new Vertex[]{
            //    new Vertex(vector0, color4),
            //    new Vertex(vector4, color4),
            //    new Vertex(vector7, color4),
            //    new Vertex(vector3, color4),

            //    new Vertex(vector0, color3),
            //    new Vertex(vector1, color3),
            //    new Vertex(vector5, color3),
            //    new Vertex(vector4, color3),

            //    new Vertex(vector4, color2),
            //    new Vertex(vector5, color2),
            //    new Vertex(vector6, color2),
            //    new Vertex(vector7, color2),

            //    new Vertex(vector7, color1),
            //    new Vertex(vector6, color1),
            //    new Vertex(vector2, color1),
            //    new Vertex(vector3, color1),

            //    new Vertex(vector5, color0),
            //    new Vertex(vector1, color0),
            //    new Vertex(vector2, color0),
            //    new Vertex(vector6, color0)};

            //return vertices;
        }
        private static Vector3[] GenerateColors()
        {
            Vector3[] output = new Vector3[COLOR_COUNT];
            Color4[] temp = new Color4[COLOR_COUNT] {
                Color4.Red,
                Color4.Cyan,
                Color4.Blue,
                Color4.Magenta,
                Color4.Yellow,
                Color4.LawnGreen};

            for (int i = 0; i < COLOR_COUNT; i++)
            {
                output[i] = new Vector3(temp[i].R, temp[i].G, temp[i].B);
            }

            return output;
        }
        private void HandleCollision()
        {
            if (piece.IsOverlapping(this) | piece.IsCollidingWithFloor())
            {
                switch (prevAction)
                {
                    case Actions.Down:
                        piece.Y++;
                        SetPiece();
                        break;
                    case Actions.Left:
                        piece.X++;
                        break;
                    case Actions.Right:
                        piece.X--;
                        break;
                    case Actions.RotateCW:
                        piece.RotateCC();
                        break;
                    case Actions.RotateCC:
                        piece.RotateCW();
                        break;
                    default:
                        break;
                }
                
            }
        }
        private void SetPiece()
        {
            piece.SetInPlace(this);
            ScanForCompleteRows();
            piece = CreateRandomPiece();
        }
        private void ScanForCompleteRows()
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                bool complete = true;

                for (int x = 0; x < WIDTH; x++)
                {
                    if (!GetStateValue(x, y).occupied)
                    {
                        complete = false;
                        break;
                    }
                }

                //Push down upper rows
                if (complete)
                {
                    for (int uppery = y + 1; uppery < HEIGHT; uppery++)
                    {
                        for (int upperx = 0; upperx < WIDTH; upperx++)
                        {
                            SetStateValue(upperx, uppery - 1, GetStateValue(upperx, uppery));
                        }
                    }
                    y--;
                }
            }
        }
    }

    enum Actions
    {
        Down,
        Left,
        Right,
        RotateCW,
        RotateCC
    }
}
