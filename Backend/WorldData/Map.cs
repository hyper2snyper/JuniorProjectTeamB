using JuniorProject.Frontend.Components;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using JuniorProject.Backend.Helpers;
using System.IO;

namespace JuniorProject.Backend.WorldData
{
    public class Map
    {
        Vector2Int mapPixelSize;
        public Bitmap worldImage;

        uint seedInt;
        string seed;
        public string Seed
        {
            get { return seed; }
        }

        float amp, freq;
        int octaves;
        public float seaLevel;
        public float treeLine;

        public class BiomeData
        {
            public string name;
            public Color tileColorLow;
            public Color tileColorHigh;
            public int movementCost;
            public float heightMin;
            public float heightMax;
            public float amp;
            public float freq;
            public int octaves;
            public bool ignoreNoise = false;
            public BiomeData? requiredBiome = null;
            //Resource Data
            public Dictionary<string, int> resourceData = new Dictionary<string, int>();
        }
        static Dictionary<string, BiomeData> biomeList = new Dictionary<string, BiomeData>(); //Loaded terrains from the DB
        static BiomeData defaultBiome;
        public static void LoadTerrain()
        {
            biomeList = new Dictionary<string, BiomeData>();

            Debug.Print("Loading Biome Data...");
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Biomes;");
            Dictionary<BiomeData, string> biomeLinking = new Dictionary<BiomeData, string>();
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
                terrain.amp = results.GetFloat(6);
                terrain.freq = results.GetFloat(7);
                terrain.octaves = results.GetInt32(8);
                terrain.ignoreNoise = results.GetBoolean(9);
                if (!results.IsDBNull(10))
                {
                    biomeLinking.Add(terrain, results.GetString(10));
                }

                //Get Resource Data for Biome
                SQLiteDataReader resourceResults = DatabaseManager.ReadDB("SELECT * FROM BiomeResource;");
                while (resourceResults.Read())
                {
                    if (resourceResults.GetString(0) == terrain.name)
                    {
                        string dbResource = resourceResults.GetString(1);
                        terrain.resourceData[dbResource] = resourceResults.GetInt32(2);
                    }
                }
                biomeList.Add(terrain.name, terrain);
                defaultBiome ??= terrain;
            }
            foreach (KeyValuePair<BiomeData, string> biomeLinkPair in biomeLinking)
            {
                biomeLinkPair.Key.requiredBiome = biomeList[biomeLinkPair.Value];
            }
        }



        BiomeData?[,] biomeMap;
        public BiomeData?[,] BiomeMap { get { return biomeMap; } }
        public float[,] heightMap;

        public Map()
        {

        }

        public Map(Vector2Int mapPixelSize, string seed, float freq, float amp, int octaves, float seaLevel, float treeLine)
        {
            this.mapPixelSize = mapPixelSize;
            if (seed == null)
            {
                seedInt = (uint)DateTime.Now.Ticks;
                this.seed = seedInt.ToString();
            }
            else if (uint.TryParse(seed, out seedInt)) //See if the inputted seed is just a number
            {
                this.seed = seedInt.ToString(); //It is.
            }
            else
            {
                this.seed = seed;
                seedInt = 0;
                foreach (char c in seed)
                {
                    seedInt += c;
                }
            }
            this.freq = freq;
            this.amp = amp;
            this.octaves = octaves;
            this.seaLevel = seaLevel;
            this.treeLine = treeLine;
            worldImage = new Bitmap(mapPixelSize.X, mapPixelSize.Y);

            biomeMap = new BiomeData[mapPixelSize.X, mapPixelSize.Y];
            heightMap = new float[mapPixelSize.X, mapPixelSize.Y];
        }


        public void SaveMap(Serializer serializer)
        {
            serializer.arbitraryPostWrite += (ref FileStream s) =>
            {
                worldImage.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            };
        }

        public void LoadMap(Bitmap bitmap)
        {
            worldImage = bitmap;
            ClientCommunicator.RegisterData<Bitmap>("WorldImage", worldImage);
        }

        public void GenerateMap()
        {
            Debug.Print("Generating Heightmap...");
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating Heightmap...", true);
            GenerateHeightMap();
            Debug.Print("Generating Image...");
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating Image...", true);
            GenerateImage();
        }

        public void GenerateHeightMap()
        {
            heightMap = Perlin.GenerateNoise(new Vector2Int(mapPixelSize.X, mapPixelSize.Y), (int)seedInt, amp, freq, octaves);

            Dictionary<BiomeData, float[,]> biomePainting = new Dictionary<BiomeData, float[,]>();

            foreach (BiomeData biome in biomeList.Values)
            {
                biomePainting.Add(biome,
                    Perlin.GenerateNoise(
                        new Vector2Int(mapPixelSize.X, mapPixelSize.Y),
                        (int)seedInt,
                        biome.amp,
                        biome.freq,
                        biome.octaves));
            }

            for (int x = 0; x < mapPixelSize.X; x++)
            {
                for (int y = 0; y < mapPixelSize.Y; y++)
                {
                    if (heightMap[x, y] < seaLevel || heightMap[x, y] > treeLine) //Oceans and mountains have no biome.
                    {
                        biomeMap[x, y] = null;
                        continue;
                    }
                    float elevation = (heightMap[x, y] + seaLevel) * (1 - seaLevel);
                    biomeMap[x, y] = defaultBiome;
                    foreach (KeyValuePair<BiomeData, float[,]> biomePaint in biomePainting)
                    {
                        if (biomePaint.Value[x, y] < 0 && !biomePaint.Key.ignoreNoise) continue;
                        if (biomePaint.Key.requiredBiome != null)
                        {
                            if (biomeMap[x, y].name != biomePaint.Key.requiredBiome.name) continue;
                            biomeMap[x, y] = biomePaint.Key;
                            continue;
                        }
                        if (elevation < biomePaint.Key.heightMin) continue;
                        if (elevation > biomePaint.Key.heightMax) continue;
                        biomeMap[x, y] = biomePaint.Key;
                    }
                }
            }
        }

        public void GenerateImage()
        {
            for (int x = 0; x < mapPixelSize.X; x++)
            {
                for (int y = 0; y < mapPixelSize.Y; y++)
                {
                    int r = 0, g = 0, b = 0;
                    if (biomeMap[x, y] != null)
                    {
                        float elevation = (heightMap[x, y] + seaLevel) * (1 - seaLevel);
                        r = (int)MathH.Lerp(biomeMap[x, y].tileColorLow.R, biomeMap[x, y].tileColorHigh.R, elevation);
                        g = (int)MathH.Lerp(biomeMap[x, y].tileColorLow.G, biomeMap[x, y].tileColorHigh.G, elevation);
                        b = (int)MathH.Lerp(biomeMap[x, y].tileColorLow.B, biomeMap[x, y].tileColorHigh.B, elevation);
                    }
                    if (heightMap[x, y] < seaLevel)
                    {
                        b = (int)(255 * ((heightMap[x, y] * 0.5) + 0.5));
                    }
                    if (heightMap[x, y] > treeLine)
                    {
                        float scale = ((heightMap[x, y] * 0.5f) + 0.5f);

                        r = (int)(200 * scale);
                        g = (int)(200 * scale);
                        b = (int)(200 * scale);
                    }
                    r = int.Clamp(r, 0, 255);
                    g = int.Clamp(g, 0, 255);
                    b = int.Clamp(b, 0, 255);

                    worldImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

        }
        public int GetBiomeResources(string biomeName, string resourceName)
        {
            return biomeList[biomeName].resourceData[resourceName];
        }

        public Dictionary<string, int> GetBiomeResources(string biomeName)
        {
            return biomeList[biomeName].resourceData;
        }
    }
}