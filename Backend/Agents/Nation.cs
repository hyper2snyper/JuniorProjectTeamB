using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    class Nation : Serializable
    {
        public string name = "";
        public string color = "";

        TileMap tileMap;

        List<Unit> units = new List<Unit>();
        List<Building> buildings = new List<Building>();
        List<TileMap.Tile> territory = new List<TileMap.Tile>();

        World world;

        public Nation(string name, string color, TileMap tileMap, World world)
        {
            this.name = name;
            this.color = color;
            this.tileMap = tileMap;
            this.world = world;
        }

        public Nation(string name, string color, TileMap tileMap, int quadrant, World world) : this(name, color, tileMap, world)
        {
            PlaceStart(quadrant);   
        }

        public void PlaceStart(int quadrant)
        {
            Vector2Int startingPos = new Vector2Int();
            switch(quadrant)
            {
                case 0: startingPos = new Vector2Int((tileMap.mapSize.X / 4), (tileMap.mapSize.Y/ 4)); break;
                case 1: startingPos = new Vector2Int((tileMap.mapSize.X/2) + (tileMap.mapSize.X/4), (tileMap.mapSize.Y / 4)); break;
                case 2: startingPos = new Vector2Int((tileMap.mapSize.X / 4), (tileMap.mapSize.Y / 2) + (tileMap.mapSize.Y / 4)); break;
                case 3: startingPos = new Vector2Int((tileMap.mapSize.X / 2) + (tileMap.mapSize.X / 4), (tileMap.mapSize.Y / 2) + (tileMap.mapSize.Y / 4)); break;
            }
            
            TileMap.Tile tile = tileMap.getTile(startingPos);
            for(int x = -1; x < 1; x++)
            {
                for(int y = -1; y < 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    for(int i = 1; tile == null || tile.impassible; i++)
                    {
                        if (startingPos.X + (x * i) > tileMap.mapSize.X || startingPos.X + (x * i) < 0) break;
						if (startingPos.Y + (y * i) > tileMap.mapSize.Y || startingPos.Y + (y * i) < 0) break;
						tile = tileMap.getTile(new Vector2Int(startingPos.X + (x*i), startingPos.Y + (y*i)));
                    }
                }
            }
            TileMap.Tile[,] tiles;
            TileMap.Tile? currentLand = tile;
            TileMap.Tile? nearestLand = null;
			int passables = 0;
            do
            {
                tiles = tileMap.getPassableTileNeighbors(currentLand);
                foreach (TileMap.Tile t in tiles)
                {
                    if (nearestLand == null) nearestLand = t;
                    if (t == null) continue;
                    passables++;
                }
                currentLand = nearestLand;
            } while (passables < 8);
			tileMap.convertTile(currentLand.pos, color);
            Unit unit = new Unit(Unit.unitTemplates.Keys.First(), color, world, currentLand.pos);
            AddUnit(unit);
            world.unitManager.AddUnit($"{unit.getSpriteName()}.{1}", unit);
        }

        public void CalculateObjectives()
        {

        }

        public void TakeTurn(ulong tick)
        {

        }

        public void AddTerritory(TileMap.Tile tile)
        {
            tileMap.convertTile(tile.pos, color);
            territory.Add(tile);
        }

        public void RemoveTerritory(TileMap.Tile tile)
        {
            tileMap.convertTile(tile.pos, "");
            territory.Remove(tile);
        }

        public void AddBuilding(Building building)
        {

        }

        public void RemoveBuilding(Building building)
        {

        }

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {

        }

		public override void SerializeFields()
		{
			throw new NotImplementedException();
		}

		public override void DeserializeFields()
		{
			throw new NotImplementedException();
		}
	}
}
