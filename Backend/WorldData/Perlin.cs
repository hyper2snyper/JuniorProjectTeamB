using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    internal class Perlin
    {
        static uint seed;

        struct Vector2
        {
            public float x, y;
        }


        /* Function to linearly interpolate between a0 and a1
		* Weight w should be in the range [0.0, 1.0]
		*/
        static float Interpolate(float a0, float a1, float w)
        {
            return (a1 - a0) * ((w * (w * 6.0f - 15.0f) + 10.0f) * w * w * w) + a0;
        }


        static Vector2 RandomGradient(int ix, int iy)
        {
            const uint w = 8 * sizeof(uint);
            const uint s = w / 2; // rotation width
            uint a = (uint)ix, b = (uint)iy;
            a *= Perlin.seed;
            b ^= a << (int)s | a >> (int)(w - s);
            b *= 1911520717;
            a ^= b << (int)s | b >> (int)(w - s);
            a *= 2048419325;
            double random = a * (3.14159265 / ~(~0u >> 1)); // in [0, 2*Pi]
            Vector2 v;
            v.x = (float)Math.Cos(random); v.y = (float)Math.Sin(random);
            return v;
        }

        // Computes the dot product of the distance and gradient vectors.
        static float DotGridGradient(int ix, int iy, float x, float y)
        {
            // Get gradient from integer coordinates
            Vector2 gradient = RandomGradient(ix, iy);

            // Compute the distance vector
            float dx = x - (float)ix;
            float dy = y - (float)iy;

            // Compute the dot-product
            return (dx * gradient.x + dy * gradient.y);
        }

        // Compute Perlin noise at coordinates x, y
        static float GetPerlin(float x, float y)
        {
            // Determine grid cell coordinates
            int x0 = (int)Math.Floor(x);
            int x1 = x0 + 1;
            int y0 = (int)Math.Floor(y);
            int y1 = y0 + 1;

            // Determine interpolation weights
            // Could also use higher order polynomial/s-curve here
            float sx = x - (float)x0;
            float sy = y - (float)y0;

            // Interpolate between grid point gradients
            float n0, n1, ix0, ix1, value;

            n0 = DotGridGradient(x0, y0, x, y);
            n1 = DotGridGradient(x1, y0, x, y);
            ix0 = Interpolate(n0, n1, sx);

            n0 = DotGridGradient(x0, y1, x, y);
            n1 = DotGridGradient(x1, y1, x, y);
            ix1 = Interpolate(n0, n1, sx);

            value = Interpolate(ix0, ix1, sy);
            return value; // Will return in range -1 to 1. To make it in range 0 to 1, multiply by 0.5 and add 0.5
        }

        public static float[,] GeneratePerlinNoise(int width, int height, float freq, float amp, uint seed)
        {
            Perlin.seed = seed;
            float[,] map = new float[width, height];
            freq /= (height + width) / 2;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float raw = (GetPerlin(x * freq, y * freq) * amp); //Unclamped
                    if (raw > 1) raw = 1;
                    if (raw < -1) raw = -1;
                    map[x, y] = raw;
                }
            }
            return map;
        }

    }
}
