using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Helpers
{
    [DebuggerDisplay("({X}, {Y})")]
    class Vector2
    {
        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Down = new Vector2(0, -1);


        private float x, y;
        public float X { get { return x; } }
        public float Y { get { return y; } }
        public float Magnitude { get { return MathF.Sqrt((x * x) + (y * y)); } }
		public Vector2 Normalize
		{
			get
			{
				return new Vector2(X / Magnitude, Y / Magnitude);
			}
		}

		public Vector2(float x = 0, float y = 0)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }
        public static float Dot(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

        public static explicit operator Vector2Int(Vector2 v)
        {
            return new Vector2Int((int)v.X, (int)v.Y);
        }

    }
}
