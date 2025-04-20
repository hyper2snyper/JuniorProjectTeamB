using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;

namespace JuniorProject.Backend.WorldData
{
    public class TileMap : Serializable
    {
        [DebuggerDisplay("({pos.X}, {pos.Y})")]
        public class Tile : Serializable
        {
            public Vector2Int pos = new Vector2Int();

            public Dictionary<string, float> terrainPercentages = new Dictionary<string, float>();
            public float movementCost;
            public float elevationAvg;

            public bool impassible = false;
            public bool coast = false;

            public string primaryBiome;

            List<Mob> occupants = new List<Mob>();
            public List<Mob> Occupants
            {
                get { return occupants; }
            }

            Nation? _owner = null;
            public Nation? Owner
            {
                get { return _owner; }
                set
                {
                    if (_owner == null)
                    {
                        _owner = value;
                        return;
                    }

                    if (_owner == value) return;

                    _owner?.RemoveTerritory(this);
                    _owner = value;
                }
            }

            public override string ToString()
            {
                return pos.ToString();
            }

            public override void SerializeFields()
            {
                SerializeField(pos);
                SerializeField<string, float>(terrainPercentages);
                SerializeField(movementCost);
                SerializeField(elevationAvg);
                SerializeField(impassible);
                SerializeField(coast);
            }

            public override void DeserializeFields()
            {
                pos = DeserializeField<Vector2Int>();
                terrainPercentages = DeserializeDictionary<string, float>();
                movementCost = DeserializeField<float>();
                elevationAvg = DeserializeField<float>();
                impassible = DeserializeField<bool>();
                coast = DeserializeField<bool>();
            }
        }

        public Tile[,] tiles;
        public event Action TilesChanged;

        public Tile? getTile(Vector2Int v)
        {
            if (v.X < 0 || v.Y < 0) return null;
            if (v.X >= mapSize.X || v.Y >= mapSize.Y) return null;
            return tiles[v.X, v.Y];
        }

        public delegate bool neighborFilter(Tile tile);
        public Tile?[,] getTileNeighbors(Tile tile, neighborFilter? filter = null)
        {
            Tile?[,] neighbors = new Tile?[3, 3];
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Tile? t = getTile(new Vector2Int(tile.pos.X + x, tile.pos.Y + y));
                    if (filter != null && t != null)
                    {
                        if (filter(t)) continue;
                    }
                    neighbors[x + 1, y + 1] = t;
                }
            }
            return neighbors;
        }
        public Tile?[,] getPassableTileNeighbors(Tile tile)
        {
            return getTileNeighbors(tile, (Tile t) => { return t.impassible; });
        }

        public void TilesUpdated()
        {
            TilesChanged?.Invoke();
        }

        int tileSize;
        public Vector2Int mapSize;
        Vector2Int mapPixelSize;
        public string seed;

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
            this.seed = map.Seed;
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
                                if (map.heightMap[pixelPosX, pixelPosY] > map.treeLine)
                                {
                                    movementCostTotal += 5;
                                }
                                else
                                {
                                    movementCostTotal++;
                                    tile.coast = true;
                                }
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
                    float totalLandPercentage = 0;
                    foreach (string landType in landTypes.Keys)
                    {
                        float relativePercentage = landTypes[landType] / (float)(tileSize * tileSize);
                        totalLandPercentage += relativePercentage;
                        tile.terrainPercentages.Add(landType, relativePercentage);
                    }
                    if (totalLandPercentage < 0.3) tile.impassible = true;
                    tile.movementCost = movementCostTotal / (float)(tileSize * tileSize);
                    tiles[tileX, tileY] = tile;

                    string mainBiome = "";
                    foreach (var item1 in tile.terrainPercentages)
                    {
                        foreach (var item2 in tile.terrainPercentages)
                        {
                            if (item1.Value > item2.Value) mainBiome = item1.Key;
                            else if (item2.Value > item1.Value) mainBiome = item2.Key;
                        }
                    }
                    tile.primaryBiome = mainBiome;
                }
            }
            ClientCommunicator.RegisterData<TileMap>("TileMap", this);
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
            SerializeField<Tile>(tiles);
            SerializeField(seed);
        }

        public override void DeserializeFields()
        {
            tileSize = DeserializeField<int>();
            mapSize = DeserializeField<Vector2Int>();
            mapPixelSize = DeserializeField<Vector2Int>();
            tiles = Deserialize2DArray<Tile>();
            seed = DeserializeField<string>();

            ClientCommunicator.RegisterData<Vector2Int>("mapPixelSize", mapPixelSize);
            ClientCommunicator.RegisterData<int>("tileSize", tileSize);
            ClientCommunicator.RegisterData<TileMap>("TileMap", this);
        }
        public Dictionary<string, int> GetTileResource(int xPos, int yPos)
        {
            string primaryBiome = "";
            foreach (var item1 in tiles[xPos, yPos].terrainPercentages)
            {
                foreach (var item2 in tiles[xPos, yPos].terrainPercentages)
                {
                    if (item1.Value > item2.Value) primaryBiome = item1.Key;
                    else if (item2.Value > item1.Value) primaryBiome = item2.Key;
                }
            }
            return (map.GetBiomeResources(primaryBiome));
        }
    }
}