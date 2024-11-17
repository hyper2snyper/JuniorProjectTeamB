using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
		Bitmap worldImage = new Bitmap(mapheight, mapwidth);

		class TerrainData
		{
			public string name;
			public Color tileColor;
			public int movementCost;
			public float heightMin;
			public float heightMax;
		}
		List<TerrainData> terrains = new List<TerrainData>();

		~Map()
		{
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);
		}

		public Map()
		{
			LoadTerrain();
		}

		void LoadTerrain()
		{
			SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Terrain;");
			while(results.Read())
			{
				TerrainData terrain = new TerrainData();
				terrain.name = results.GetString(0);
				string unparsedRGB = results.GetString(1);
				int r = int.Parse(unparsedRGB[..3]);
				int g = int.Parse(unparsedRGB[4..7]);
				int b = int.Parse(unparsedRGB[8..10]);
				terrain.tileColor = Color.FromArgb(r, g, b);
				terrain.movementCost = results.GetInt32(2);
				terrain.heightMin = results.GetFloat(3);
				terrain.heightMax = results.GetFloat(4);
				terrains.Add(terrain);
			}
		}

		public void GenerateWorld()
		{
			
			float[,] baseTerrain = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 3, 1f);
			float[,] landBase = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 8, 0.5f);
			float[,] mountainBase = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 20, 3);

			for (int x = 0; x < mapheight; x++)
			{
				for (int y = 0; y < mapwidth; y++)
				{
					Color c = new Color();

					float terrainHeight = baseTerrain[x, y];
					if(terrainHeight > -0.01f)
					{
						terrainHeight += MathF.Max(landBase[x,y], 0);
					}
					if(terrainHeight > 0.45)
					{
						terrainHeight = MathF.Max(mountainBase[x, y], 0.45f);
					}

					float adjusted = (terrainHeight*0.5f)+0.5f;
					int adjustedInt = Math.Clamp((int)(adjusted * 255), 0, 255);
					c = Color.FromArgb(adjustedInt, adjustedInt, adjustedInt);

					worldImage.SetPixel(x, y, c);
				}
			}
		}
	}
}
