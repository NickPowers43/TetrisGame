using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using OpenGLLib;
using OpenGLLib.TetrisStuff;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TetrisGame
{
    class Window : BasicWindow
    {
        Board board;
        Stopwatch sw;

        BigInteger sum = 0;
        BigInteger samples = 0;

        public Window()
            : base(Board.BOARD_PIXEL_WIDTH, Board.BOARD_PIXEL_HEIGHT)
        {
            GL.ClearColor(Color4.White);

            board = new Board();

            sw = new Stopwatch();
            //GL.Enable(EnableCap.DepthTest);
            //VSync = VSyncMode.Off;
            //TargetRenderFrequency = 0;
            
            WindowBorder = OpenTK.WindowBorder.Fixed;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            sw.Restart();
            board.Update(Keyboard, e.Time);
            sw.Stop();
            sum += sw.ElapsedTicks;
            samples++;

            //Console.Clear();
            //Console.Write((sum / samples).ToString());
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

            board.Draw();

            SwapBuffers();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
