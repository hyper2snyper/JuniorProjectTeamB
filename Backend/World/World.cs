
using System.Drawing;

namespace JuniorProject.Backend.World
{
	class World
	{
		const int mapheight = 100;
		const int mapwidth = 100;

		Bitmap worldImage = new Bitmap(mapheight, mapwidth);


		class TerrainData
		{ 
			
		}

		public void GenerateWorldImage()
		{
		
			for(int x = 0; x < mapheight; x++)
			{
				for(int y = 0; y < mapwidth; y++) 
				{
					worldImage.SetPixel(x,y,Color.AliceBlue);
				}
			}
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);

		}

	}

}
