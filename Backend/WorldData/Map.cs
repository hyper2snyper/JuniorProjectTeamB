using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
	class Map
	{
		const int mapheight = 2000;
		const int mapwidth = 2000;



		class TerrainData
		{
			Color tileColor;
			int movementPenalty;

		}

		public void GenerateWorld(float freq, float amp)
		{
			Bitmap worldImage = new Bitmap(mapheight, mapwidth);
			float[,] perlinNoiseMap = Perlin.GeneratePerlinNoise(mapwidth, mapheight, freq, amp);


			for (int x = 0; x < mapheight; x++)
			{
				for (int y = 0; y < mapwidth; y++)
				{
					Color c = new Color();
					float n = (perlinNoiseMap[x, y]*-0.5f)+0.5f;
					int ni = Math.Clamp((int)(n * 255), 0, 255);
					c = Color.FromArgb(ni, ni, ni);

					if (perlinNoiseMap[x,y] > 0)
					{
						c = Color.Blue;
					} 
					if (perlinNoiseMap[x, y] > 0.5f)
					{
						c = Color.DarkBlue;
					}

					worldImage.SetPixel(x, y, c);
				}
			}
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);
			

		}
	}
}
