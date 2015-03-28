using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisGame
{
    class MainMethod
    {
        static void Main(string[] args)
        {
            using (Window window = new Window())
            {
                window.Run(0.0f, 0.0f);
            }

        }
    }
}
