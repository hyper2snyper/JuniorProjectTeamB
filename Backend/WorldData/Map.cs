﻿using System;
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
		const int MAP_WIDTH= 2000;
		const int MAP_HEIGHT = 2000;
		public Bitmap worldImage = new Bitmap(MAP_HEIGHT, MAP_WIDTH);

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
			terrainMap = new TerrainData[MAP_WIDTH, MAP_HEIGHT];
			heightMap = new float[MAP_WIDTH, MAP_HEIGHT];
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
		}

		public void GenerateHeightMap()
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			
			float[,] baseTerrain = Perlin.GeneratePerlinNoise(MAP_WIDTH, MAP_HEIGHT, 3, 1f, (uint) random.Next());
			float[,] landBase = Perlin.GeneratePerlinNoise(MAP_WIDTH, MAP_HEIGHT, 8, 0.5f, (uint) random.Next());
			float[,] mountainBase = Perlin.GeneratePerlinNoise(MAP_WIDTH, MAP_HEIGHT, 20, 3, (uint) random.Next());

			for (int x = 0; x < MAP_HEIGHT; x++)
			{
				for (int y = 0; y < MAP_WIDTH; y++)
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
			TerrainData selectedTerrain = terrains[0]; //This is probably not very good.
			foreach(TerrainData d in terrains)
			{
				if (height > d.heightMax || height < d.heightMin) continue;
				selectedTerrain = d;
				break;
			}
			terrainMap[x, y] = selectedTerrain;
		}

		public void GenerateImage()
		{
			for(int x = 0; x < MAP_WIDTH; x++)
			{
				for(int y = 0; y < MAP_HEIGHT; y++)
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

		public void SaveImage()
		{
			worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
