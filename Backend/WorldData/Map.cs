using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using JuniorProject.Backend.Helpers;

namespace JuniorProject.Backend.WorldData
{
    class Map
    {
        const int MAP_PIXEL_WIDTH = 1000;
        const int MAP_PIXEL_HEIGHT = 1000;
        public Bitmap worldImage = new Bitmap(MAP_PIXEL_HEIGHT, MAP_PIXEL_WIDTH);


        public float seaLevel = 0f;
        public float treeLine = 0.8f;

        public class BiomeData
        {
            public string name;
            public Color tileColorLow;
            public Color tileColorHigh;
            public int movementCost;
            public float heightMin;
            public float heightMax;
        }
        List<BiomeData> biomeList = new List<BiomeData>(); //Loaded terrains from the DB
        


        BiomeData?[,] biomeMap;
        float[,] heightMap;


        public class Tile 
        {
            public Dictionary<string, float> terrainPercentages = new Dictionary<string, float>();
            public int movementCost;
            public float elevationAvg;
		}


        const int TILE_SIZE = 20;
        int mapHeight, mapWidth;
        Tile[,] tiles;
        public Tile getTile(int x, int y)
        {
            return tiles[x, y];
        }

        public Map()
        {
            Debug.Print("Loading Biome Data...");
            LoadTerrain();
            biomeMap = new BiomeData[MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT];
            heightMap = new float[MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT];
            mapWidth = MAP_PIXEL_WIDTH / TILE_SIZE;
            mapHeight = MAP_PIXEL_HEIGHT / TILE_SIZE;
            tiles = new Tile[mapWidth, mapHeight];
        }

        ~Map() { }

        public void SaveMap(Serializer serializable)
        {
            for (int tileX = 0; tileX < mapWidth; tileX++)
            {
                for (int tileY = 0; tileY < mapHeight; tileY++)
                {
                    
                }
            }
        }

        void LoadTerrain()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Biomes;");
            while (results.Read())
            {
                BiomeData terrain = new BiomeData();
                terrain.name = results.GetString(0);
                string unparsedRGB = results.GetString(1);
                int r = int.Parse(unparsedRGB[..3]);
                int g = int.Parse(unparsedRGB[4..7]);
                int b = int.Parse(unparsedRGB[8..11]);
                terrain.tileColorLow = Color.FromArgb(255, r, g, b);
				unparsedRGB = results.GetString(2);
				r = int.Parse(unparsedRGB[..3]);
				g = int.Parse(unparsedRGB[4..7]);
				r = int.Parse(unparsedRGB[8..11]);
				terrain.tileColorHigh = Color.FromArgb(255, r, g, b);
				terrain.movementCost = results.GetInt32(3);
                terrain.heightMin = results.GetFloat(4);
                terrain.heightMax = results.GetFloat(5);
                biomeList.Add(terrain);
            }

        }

        public void GenerateWorld()
        {
            Debug.Print("Generating Heightmap...");
            GenerateHeightMap();
            Debug.Print("Generating Image...");
            GenerateImage();
            Debug.Print("Generating Tiles...");
            GenerateTiles();
        }

        public void GenerateHeightMap()
        {
            int seed = (int)DateTime.Now.Ticks;

			heightMap = Perlin.GenerateNoise(new Vector2Int(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT), seed, 2, 0.002f, 8);
			
            Dictionary<BiomeData, float[,]> biomePainting = new Dictionary<BiomeData, float[,]>();

            for(int i = 1; i < biomeList.Count; i++)
            {
                biomePainting.Add(biomeList[i], Perlin.GenerateNoise(new Vector2Int(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT), seed, 2, 0.009f, 8));
			}

			for (int x = 0; x < MAP_PIXEL_WIDTH; x++)
            {
                for(int y = 0;  y < MAP_PIXEL_HEIGHT; y++)
                {
                    if (heightMap[x, y] < seaLevel || heightMap[x, y] > treeLine) //Oceans and mountains have no biome.
                    {
                        biomeMap[x,y] = null;
                        continue;
                    }
					float elevation = (heightMap[x, y] + seaLevel) * (1 - seaLevel);
                    biomeMap[x, y] = biomeList[0];                    
					foreach (KeyValuePair<BiomeData, float[,]> biomePaint in biomePainting)
                    {
                        if (biomePaint.Value[x, y] < 0) continue;
                        if (elevation < biomePaint.Key.heightMin) continue;
                        if (elevation > biomePaint.Key.heightMax) continue;
                        biomeMap[x, y] = biomePaint.Key;
                        break;
                    }
                }
            }
		}

		public void ApplyTerrain(int x, int y, float height)
        {
            BiomeData selectedTerrain = biomeList[0]; //This is probably not very good.
            foreach (BiomeData d in biomeList)
            {
                if (height > d.heightMax || height < d.heightMin) continue;
                selectedTerrain = d;
                break;
            }
            biomeMap[x, y] = selectedTerrain;
        }

        public void GenerateImage()
        {
            for (int x = 0; x < MAP_PIXEL_WIDTH; x++)
            {
                for (int y = 0; y < MAP_PIXEL_HEIGHT; y++)
                {
                    int r = 0, g = 0, b = 0;
					if (biomeMap[x,y] != null)
                    {
						float elevation = (heightMap[x, y] + seaLevel) * (1 - seaLevel);
						r = (int)MathH.Lerp(biomeMap[x,y].tileColorLow.R, biomeMap[x,y].tileColorHigh.R, elevation);
						g = (int)MathH.Lerp(biomeMap[x, y].tileColorLow.G, biomeMap[x, y].tileColorHigh.G, elevation);
						b = (int)MathH.Lerp(biomeMap[x, y].tileColorLow.B, biomeMap[x, y].tileColorHigh.B, elevation);
					}
                    if (heightMap[x,y] < seaLevel)
                    {
                        b = (int)(255 * ((heightMap[x, y]*0.5)+0.5));
                    }
                    if (heightMap[x,y] > treeLine)
                    {
                        r = (int)(175 * ((heightMap[x, y] * 0.5) + 0.5));
						g = (int)(175 * ((heightMap[x, y] * 0.5) + 0.5));
						b = (int)(175 * ((heightMap[x, y] * 0.5) + 0.5));
					}
                    r = int.Clamp(r, 0, 255);
                    g = int.Clamp(g, 0, 255);
                    b = int.Clamp(b, 0, 255);

                    worldImage.SetPixel(x, y, Color.FromArgb(r,g,b));
				}
            }
        }

        public void SaveImage()
        {
            worldImage.Save($"{Properties.Resources.ProjectDir}\\LocalData\\Map.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public void GenerateTiles()
        {
            for (int tileX = 0; tileX < mapWidth; tileX++)
            {
                for (int tileY = 0; tileY < mapHeight; tileY++)
                {
                    Tile tile = new Tile();
                    Dictionary<string, int> landTypes = new Dictionary<string, int>();
                    int movementCostTotal = 0;

                    for (int x = 0; x < TILE_SIZE; x++)
                    {
                        for (int y = 0; y < TILE_SIZE; y++)
                        {
                            int pixelPosX = (tileX * TILE_SIZE) + x;
                            int pixelPosY = (tileY * TILE_SIZE) + y;
							if (pixelPosX > MAP_PIXEL_WIDTH) continue;
                            if (pixelPosY > MAP_PIXEL_HEIGHT) continue;
							if (biomeMap[pixelPosX, pixelPosY] == null) continue;

							string landType = biomeMap[pixelPosX, pixelPosY].name;
                            if (landTypes.ContainsKey(landType))
                            {
                                landTypes[landType]++;
                            }
                            else
                            {
                                landTypes.Add(landType, 1);
                            }
                            movementCostTotal += biomeMap[pixelPosX, pixelPosY].movementCost;
                        }
                    }

                    foreach (string landType in landTypes.Keys)
                    {
                        float relativePercentage = landTypes[landType] / (TILE_SIZE * TILE_SIZE);
                        tile.terrainPercentages.Add(landType, relativePercentage);
                    }
                    tile.movementCost = movementCostTotal / (TILE_SIZE * TILE_SIZE);
                    tiles[tileX, tileY] = tile;
                }
            }
        }
    }

}
