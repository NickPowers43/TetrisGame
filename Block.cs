using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace TetrisGame
{
    struct Block
    {
        public bool occupied;
        public Vector3 color;

        public Block(Vector3 color, bool occupied)
        {
            this.occupied = occupied;
            this.color = color;
        }

        public static Block EMPTY
        {
            get
            {
                return new Block();
            }
        }

        public static Block[] ConvertFromBoolArray(bool[] bools, Vector3 color)
        {
            Block[] output = new Block[bools.Length];
            for (int i = 0; i < bools.Length; i++)
            {
                output[i] = new Block(color, false);
            }
            return output;
        }
    }
}
