using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    class TileMap
    {
        public class Tile
        {
            public Dictionary<string, float> terrainPercentages = new Dictionary<string, float>();
            public int movementCost;
            public float elevationAvg;
        }

        Tile[,] tiles;
        public Tile getTile(int x, int y)
        {
            return tiles[x, y];
        }

        int tileSize;
        Vector2Int mapSize;
        Vector2Int mapPixelSize;

        Map map;

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
                            if (map.BiomeMap[pixelPosX, pixelPosY] == null) continue;

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

        List<string> GetTileResource(int xPos, int yPos)
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

        List<int> GetTileRate(int xPos, int yPos)
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
            return (map.GetBiomeRate(primaryBiome));
        }

    }
}