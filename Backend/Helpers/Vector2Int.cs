using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Helpers
{
    [DebuggerDisplay("({X}, {Y})")]
    public class Vector2Int
    {
        public static readonly Vector2Int Zero = new Vector2Int(0, 0);
        public static readonly Vector2Int Right = new Vector2Int(1, 0);
        public static readonly Vector2Int Left = new Vector2Int(-1, 0);
        public static readonly Vector2Int Up = new Vector2Int(0, 1);
        public static readonly Vector2Int Down = new Vector2Int(0, -1);


        private int x, y;
        public int X { get { return x; } }
        public int Y { get { return y; } }
        public int Magnitude { get { return (int)MathF.Sqrt((x * x) + (y * y)); } }

        public Vector2Int((int, int) pos)
        {
            this.x = pos.Item1;
            this.y = pos.Item2;
        }
        public Vector2Int(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2Int(Vector2Int v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public static Vector2Int operator +(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2Int(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2Int operator -(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2Int(v1.x - v2.x, v1.y - v2.y);
        }
        public static int Dot(Vector2Int v1, Vector2Int v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

		public override string ToString()
		{
            return $"({X}, {Y})";
		}

	}
}
