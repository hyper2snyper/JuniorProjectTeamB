using JuniorProject.Frontend.Components;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace JuniorProject.Backend.WorldData
{
    class Map
    {
        const int MAP_PIXEL_WIDTH = 2000;
        const int MAP_PIXEL_HEIGHT = 2000;
        public Bitmap worldImage = new Bitmap(MAP_PIXEL_HEIGHT, MAP_PIXEL_WIDTH);

        public class TerrainData
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


        public class Tile : Serializable
        {
            public Dictionary<string, float> terrainPercentages = new Dictionary<string, float>();
            public int movementCost;

            public override int fieldCount { get { return -1; } }

            public override void SerializeFields()
            {
                SerializeField(movementCost);
                SerializeDictionary(terrainPercentages);
            }

            public override void DeserializeFields()
            {
                movementCost = DeserializeField<int>();
                terrainPercentages = DeserializeDictionary<string, float>();
            }
        }

        const int TILE_SIZE = 50;
        int mapHeight, mapWidth;
        Tile[,] tiles;
        public Tile getTile(int x, int y)
        {
            return tiles[x, y];
        }

        public Map()
        {
            Debug.Print("Registering TILE_SIZE, MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT into Client Communicator...");
            ClientCommunicator.RegisterData<int>("TILE_SIZE", TILE_SIZE);
            ClientCommunicator.RegisterData<int>("MAP_PIXEL_WIDTH", MAP_PIXEL_WIDTH);
            ClientCommunicator.RegisterData<int>("MAP_PIXEL_HEIGHT", MAP_PIXEL_HEIGHT);
            Debug.Print("Loading Terrain Data...");
            LoadTerrain();
            terrainMap = new TerrainData[MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT];
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
                    serializable.SaveObject(getTile(tileX, tileY));
                }
            }
        }

        void LoadTerrain()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Terrain;");
            while (results.Read())
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
                if (terrain.landType == "Ocean" && terrain.heightMax > oceanHeightMax) oceanHeightMax = terrain.heightMax;
                if (terrain.landType == "Mountain" && terrain.heightMin < highlandMin) highlandMin = terrain.heightMin;
                terrains.Add(terrain);
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
            Random random = new Random((int)DateTime.Now.Ticks);

            float[,] baseTerrain = Perlin.GeneratePerlinNoise(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT, 3, 1f, (uint)random.Next());
            float[,] landBase = Perlin.GeneratePerlinNoise(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT, 8, 0.5f, (uint)random.Next());
            float[,] mountainBase = Perlin.GeneratePerlinNoise(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT, 20, 3, (uint)random.Next());

            for (int x = 0; x < MAP_PIXEL_HEIGHT; x++)
            {
                for (int y = 0; y < MAP_PIXEL_WIDTH; y++)
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
            foreach (TerrainData d in terrains)
            {
                if (height > d.heightMax || height < d.heightMin) continue;
                selectedTerrain = d;
                break;
            }
            terrainMap[x, y] = selectedTerrain;
        }

        public void GenerateImage()
        {
            for (int x = 0; x < MAP_PIXEL_WIDTH; x++)
            {
                for (int y = 0; y < MAP_PIXEL_HEIGHT; y++)
                {
                    float adjustedHeight = heightMap[x, y];
                    adjustedHeight = (adjustedHeight / 2) + 0.5f; //set the value between 0 and 1

                    Color terrainColor = terrainMap[x, y].tileColor;
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

                            string landType = terrainMap[pixelPosX, pixelPosY].name;
                            if (landTypes.ContainsKey(landType))
                            {
                                landTypes[landType]++;
                            }
                            else
                            {
                                landTypes.Add(landType, 1);
                            }
                            movementCostTotal += terrainMap[x, y].movementCost;
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
