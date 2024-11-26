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
		const int mapwidth = 2000;
		const int mapheight = 2000;
		Bitmap worldImage = new Bitmap(mapheight, mapwidth);

		class TerrainData
		{
			public string name;
			public Color tileColor;
			public int movementCost;
			public float heightMin;
			public float heightMax;
			public string landType;
		}
		List<TerrainData> terrains = new List<TerrainData>(); //Loaded terrains from the DB
		float oceanHeightMax = -1;
		float highlandMin = 1;


		TerrainData[,] terrainMap;
		float[,] heightMap;

		~Map(){}

		public Map()
		{
			LoadTerrain();
			terrainMap = new TerrainData[mapwidth, mapheight];
			heightMap = new float[mapwidth, mapheight];
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
				int b = int.Parse(unparsedRGB[8..11]);
				terrain.tileColor = Color.FromArgb(255, r, g, b);
				terrain.movementCost = results.GetInt32(2);
				terrain.heightMin = results.GetFloat(3);
				terrain.heightMax = results.GetFloat(4);
				terrain.landType = results.GetString(5);
				if(terrain.landType == "Ocean" && terrain.heightMax > oceanHeightMax) oceanHeightMax = terrain.heightMax;
				if (terrain.landType == "Highland" && terrain.heightMin < highlandMin) highlandMin = terrain.heightMin;
				terrains.Add(terrain);
			}
			
		}

		public void GenerateWorld()
		{
			GenerateHeightMap();
			GenerateImage();
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);
		}

		public void GenerateHeightMap()
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			
			float[,] baseTerrain = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 3, 1f, (uint) random.Next());
			float[,] landBase = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 8, 0.5f, (uint) random.Next());
			float[,] mountainBase = Perlin.GeneratePerlinNoise(mapwidth, mapheight, 20, 3, (uint) random.Next());

			for (int x = 0; x < mapheight; x++)
			{
				for (int y = 0; y < mapwidth; y++)
				{
					float terrainHeight = baseTerrain[x, y];
					if (terrainHeight > oceanHeightMax)
					{
						terrainHeight += MathF.Max(landBase[x, y], 0);
					}
					if (terrainHeight > highlandMin)
					{
						terrainHeight = MathF.Max(mountainBase[x, y], 0.45f);
					}

					heightMap[x, y] = terrainHeight;
					ApplyTerrain(x, y, terrainHeight);
				}
			}
		}

		public void ApplyTerrain(int x, int y, float height)
		{
			TerrainData selected_terrain = terrains[0]; //This is probably not very good.
			foreach(TerrainData d in terrains)
			{
				if (height > d.heightMax || height < d.heightMin) continue;
				selected_terrain = d;
				break;
			}
			terrainMap[x, y] = selected_terrain;
		}

		public void GenerateImage()
		{
			for(int x = 0; x < mapwidth; x++)
			{
				for(int y = 0; y < mapheight; y++)
				{
					float adjustedHeight = heightMap[x,y];
					adjustedHeight = (adjustedHeight / 2) + 0.5f; //set the value between 0 and 1
					
					Color terrainColor = terrainMap[x,y].tileColor;
					Color pixelColor = Color.FromArgb(
						(int)(terrainMap[x, y].tileColor.R * adjustedHeight), 
						(int)(terrainMap[x, y].tileColor.G * adjustedHeight), 
						(int)(terrainMap[x, y].tileColor.B * adjustedHeight));

					worldImage.SetPixel(x, y, pixelColor);
				}
			}
		}
	}
}
