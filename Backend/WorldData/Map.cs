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
		const int mapheight = 1500;
		const int mapwidth = 1500;

		Bitmap worldImage = new Bitmap(mapheight, mapwidth);


		class TerrainData
		{

		}

		public void GenerateWorldImage()
		{

			for (int x = 0; x < mapheight; x++)
			{
				for (int y = 0; y < mapwidth; y++)
				{
					worldImage.SetPixel(x, y, Color.Red);
				}
			}
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);

		}
	}
}
