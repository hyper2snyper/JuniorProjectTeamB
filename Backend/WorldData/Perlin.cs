using JuniorProject.Backend.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JuniorProject.Backend.WorldData
{
	internal class Perlin
	{

		static Random r;
		static byte[] permutation = new byte[512];


		static void populatePermutation()
		{
			for(int i = 0; i < 256; i++)
			{
				permutation[i] = (byte)(r.NextDouble() * 255);
				permutation[256 + i] = permutation[i];
			}

		}

		static Vector2 RandomGradient(Vector2Int pos)
		{
			Vector2Int clampedPos = new Vector2Int(pos.X % 254, pos.Y % 254);
			float random = permutation[permutation[clampedPos.X+1] + clampedPos.Y+1];

			float randomAngle = (random/255) * 6.283185307f; //6.283185307 is 2pi
			return new Vector2(MathF.Cos(randomAngle), MathF.Sin(randomAngle));
		}

		// Computes the dot product of the distance and gradient vectors.
		static float DotGridGradient(Vector2Int i, Vector2 v)
		{
			// Get gradient from integer coordinates
			Vector2 gradient = RandomGradient(i);

			// Compute the distance vector
			float dx = v.X - (float)i.X;
			float dy = v.Y - (float)i.Y;

			// Compute the dot-product
			return (dx * gradient.X + dy * gradient.Y);
		}

		static float Interpolate(float a0, float a1, float w)
		{
			return (a1 - a0) * ((w * (w * 6.0f - 15.0f) + 10.0f) * w * w * w) + a0;
		}

		static float Noise(Vector2 pos)
		{
			// Determine grid cell coordinates
			int x0 = (int)Math.Floor(pos.X);
			int x1 = x0 + 1;
			int y0 = (int)Math.Floor(pos.Y);
			int y1 = y0 + 1;

			// Determine interpolation weights
			// Could also use higher order polynomial/s-curve here
			float sx = pos.X - x0;
			float sy = pos.Y - y0;

			// Interpolate between grid point gradients
			float n0, n1, ix0, ix1, value;

			n0 = DotGridGradient(new Vector2Int(x0, y0), pos);
			n1 = DotGridGradient(new Vector2Int(x1, y0), pos);
			ix0 = Interpolate(n0, n1, sx);

			n0 = DotGridGradient(new Vector2Int(x0, y1), pos);
			n1 = DotGridGradient(new Vector2Int(x1, y1), pos);
			ix1 = Interpolate(n0, n1, sx);

			value = Interpolate(ix0, ix1, sy);
			if(value < -1) 
				value = -1;
			if(value > 1) 
				value = 1;
			
			return value; // Will return in range -1 to 1. To make it in range 0 to 1, multiply by 0.5 and add 0.5
		}

		
		public static float FractalBrown(float ampBase, float freq, int octaves, Vector2Int pos)
		{
			float result = 0;
			float amp = ampBase;
			for(int o = 0; o < octaves; o++)
			{
				float noise = amp * Noise(new Vector2(freq * pos.X, freq * pos.Y));
				result += noise;
				amp *= 0.5f;
				freq *= 2;
			}
			if(result > ampBase) 
				result = ampBase;
			if(result < -ampBase) 
				result = -ampBase;
			return result;
		}


		public static float[,] GenerateNoise(Vector2Int size, int seed, float amp, float freq, int octaves)
		{
			r = new Random(seed);
			populatePermutation();

			float[,] noiseMap = new float[size.X, size.Y];
			for(int x = 0; x < size.X; x++)
			{
				for(int y = 0; y < size.Y; y++)
				{
					noiseMap[x, y] = FractalBrown(amp, freq, octaves, new Vector2Int(x,y));
				}
			}
			return noiseMap;
		}
	}
}
