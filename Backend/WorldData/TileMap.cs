using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    public class TileMap : Serializable
    {
        [DebuggerDisplay("({pos.X}, {pos.Y})")]
        public class Tile : Serializable
        {
            public Vector2Int pos = new Vector2Int();

            public Dictionary<string, float> terrainPercentages = new Dictionary<string, float>();
            public int movementCost;
            public float elevationAvg;

            public bool impassible = false;

            public override void SerializeFields()
            {
                SerializeField(pos);
                SerializeField(terrainPercentages);
                SerializeField(movementCost);
                SerializeField(elevationAvg);
                SerializeField(impassible);
            }

            public override void DeserializeFields()
            {
                pos = DeserializeField<Vector2Int>();
                terrainPercentages = DeserializeDictionary<string, float>();
                movementCost = DeserializeField<int>();
                elevationAvg = DeserializeField<float>();
                impassible = DeserializeField<bool>();
            }
        }

        Tile[,] tiles;
        public Tile? getTile(int x, int y)
        {
            if (x < 0 || y < 0) return null;
            if (x >= mapSize.X || y >= mapSize.Y) return null;
            return tiles[x, y];
        }
        public Tile? getTile(Vector2Int v)
        {
            return getTile(v.X, v.Y);
        }

        public Tile?[,] getTileNeighbors(Tile tile)
        {
            Tile?[,] neighbors = new Tile?[3, 3];
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    neighbors[x + 1, y + 1] = getTile(tile.pos.X + x, tile.pos.Y + y);
                }
            }
            return neighbors;
        }

        int tileSize;
        Vector2Int mapSize;
        Vector2Int mapPixelSize;

        Map map;
        public Map Map { set { map = value; } }

        public TileMap() { }

        public TileMap(int tileSize, Vector2Int mapPixelSize, string seed, float amp, float freq, int octaves, float seaLevel, float treeLine)
        {
            ClientCommunicator.RegisterData<Vector2Int>("mapPixelSize", mapPixelSize);
            ClientCommunicator.RegisterData<int>("tileSize", tileSize);
            this.mapPixelSize = mapPixelSize;
            this.tileSize = tileSize;
            mapSize = new Vector2Int(mapPixelSize.X / tileSize, mapPixelSize.Y / tileSize);
            tiles = new Tile[mapSize.X, mapSize.Y];
            this.map = new Map(mapPixelSize, seed, freq, amp, octaves, seaLevel, treeLine);
            GenerateMap();
        }

        void GenerateMap()
        {
            map.GenerateMap();
            ClientCommunicator.RegisterData<Bitmap>("WorldImage", map.worldImage);
            Debug.Print("Generating Tiles...");
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating Tiles...", true);
            GenerateTiles();
        }

        void GenerateTiles()
        {
            for (int tileX = 0; tileX < mapSize.X; tileX++)
            {
                for (int tileY = 0; tileY < mapSize.Y; tileY++)
                {
                    Tile tile = new Tile();
                    tile.pos = new Vector2Int(tileX, tileY);
                    Dictionary<string, int> landTypes = new Dictionary<string, int>();
                    int movementCostTotal = 0;

                    for (int x = 0; x < tileSize; x++)
                    {
                        for (int y = 0; y < tileSize; y++)
                        {
                            int pixelPosX = (tileX * tileSize) + x;
                            int pixelPosY = (tileY * tileSize) + y;
                            if (pixelPosX >= mapPixelSize.X) continue;
                            if (pixelPosY >= mapPixelSize.Y) continue;
                            if (map.BiomeMap[pixelPosX, pixelPosY] == null)
                            {
                                movementCostTotal += 5; //Mountain move modifier.
                                continue;
                            }

                            string landType = map.BiomeMap[pixelPosX, pixelPosY].name;
                            if (landTypes.ContainsKey(landType))
                            {
                                landTypes[landType]++;
                            }
                            else
                            {
                                landTypes.Add(landType, 1);
                            }
                            movementCostTotal += map.BiomeMap[pixelPosX, pixelPosY].movementCost;
                        }
                    }
                    if (landTypes.Keys.Count == 0) tile.impassible = true;
                    foreach (string landType in landTypes.Keys)
                    {
                        float relativePercentage = landTypes[landType] / (tileSize * tileSize);
                        tile.terrainPercentages.Add(landType, relativePercentage);
                    }
                    tile.movementCost = movementCostTotal / (tileSize * tileSize);
                    tiles[tileX, tileY] = tile;
                }
            }
        }

        public void SaveMapImage(Serializer serializer)
        {
            map.SaveMap(serializer);
        }

        public override void SerializeFields()
        {
            SerializeField(tileSize);
            SerializeField(mapSize);
            SerializeField(mapPixelSize);
            SerializeField(tiles);
        }

        public override void DeserializeFields()
        {
            tileSize = DeserializeField<int>();
            mapSize = DeserializeField<Vector2Int>();
            mapPixelSize = DeserializeField<Vector2Int>();
            tiles = Deserialize2DArray<Tile>();

            ClientCommunicator.RegisterData<Vector2Int>("mapPixelSize", mapPixelSize);
            ClientCommunicator.RegisterData<int>("tileSize", tileSize);
        }
    }
}