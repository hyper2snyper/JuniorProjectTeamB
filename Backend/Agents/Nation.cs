using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Frontend.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    public class Nation : Serializable
    {
        public string name = "";
        public string color = "";

		World world;
        public World World { set { world = value; } }
		
        public List<Unit> units = new List<Unit>();
        public List<Building> buildings = new List<Building>();
        public List<TileMap.Tile> territory = new List<TileMap.Tile>();


        public Nation() { }
        public Nation(string name, string color, World world)
        {
            this.name = name;
            this.color = color;
            this.world = world;
        }

        public Nation(string name, string color, int quadrant, World world) : this(name, color, world)
        {
            PlaceStart(quadrant);   
        }

        public void PlaceStart(int quadrant)
        {
            Vector2Int startingPos = new Vector2Int();
            switch(quadrant)
            {
                case 0: startingPos = new Vector2Int((world.map.mapSize.X / 4), (world.map.mapSize.Y/ 4)); break;
                case 1: startingPos = new Vector2Int((world.map.mapSize.X/2) + (world.map.mapSize.X/4), (world.map.mapSize.Y / 4)); break;
                case 2: startingPos = new Vector2Int((world.map.mapSize.X / 4), (world.map.mapSize.Y / 2) + (world.map.mapSize.Y / 4)); break;
                case 3: startingPos = new Vector2Int((world.map.mapSize.X / 2) + (world.map.mapSize.X / 4), (world.map.mapSize.Y / 2) + (world.map.mapSize.Y / 4)); break;
            }
            
            TileMap.Tile tile = world.map.getTile(startingPos);
            for(int x = -1; x < 1; x++)
            {
                for(int y = -1; y < 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    for(int i = 1; tile == null || tile.impassible; i++)
                    {
                        if (startingPos.X + (x * i) > world.map.mapSize.X || startingPos.X + (x * i) < 0) break;
						if (startingPos.Y + (y * i) > world.map.mapSize.Y || startingPos.Y + (y * i) < 0) break;
						tile = world.map.getTile(new Vector2Int(startingPos.X + (x*i), startingPos.Y + (y*i)));
                    }
                    if (tile != null && tile.impassible == false) break;
                }
				if (tile != null && tile.impassible == false) break;
			}
            AddTerritory(tile);
            Building castle = new Building("Capital", world.map, tile, this);
            AddBuilding(castle);
            Unit unit = new Unit("Soldier", $"soldier{color}", this, world.map, tile);
            AddUnit(unit);
        }

        public void CalculateObjectives()
        {

        }

        public void TakeTurn(ulong tick)
        {
            foreach(Building building in buildings)
            {
                building.TakeTurn(tick);
            }

            foreach(Unit unit in units)
            {
                unit.TakeTurn(tick);
            }
        }

		public void PopulateDrawablesList(ref List<GenericDrawable> genericDrawables)
        {
			foreach (TileMap.Tile tile in territory)
			{
                genericDrawables.Add(new GenericDrawable(tile.pos, $"{color}TileCover", 0));
			}
			foreach (Building building in buildings)
			{
                building.populateDrawables(ref genericDrawables);
			}
			foreach (Unit unit in units)
            {
                unit.populateDrawables(ref genericDrawables);
            }
        }

        public List<TileMap.Tile> GetBorderingTiles() 
        {
            List<Vector2Int> directions = new List<Vector2Int>([new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1)]);
            HashSet<TileMap.Tile> borderingTiles = new HashSet<TileMap.Tile>();

            foreach (TileMap.Tile tile in territory) {
                foreach (TileMap.Tile possibleTile in world.map.getPassableTileNeighbors(tile)) {
                    if (possibleTile == null) continue;
                    if (possibleTile.Owner != this) { 
                        borderingTiles.Add(possibleTile);
                    }
                }
                //foreach (var dir in directions) {
                //    if (world.map.getTile(dir + tile.pos).Owner != this) { 
                        
                //    }
                //}
            }
            return borderingTiles.ToList<TileMap.Tile>();
        }

		public void AddTerritory(TileMap.Tile tile)
        {
            tile.Owner = this;
            territory.Add(tile);
			world.RedrawAction?.Invoke();
		}

        public void RemoveTerritory(TileMap.Tile tile)
        {
            territory.Remove(tile);
			world.RedrawAction?.Invoke();
		}

        public void AddBuilding(Building building)
        {
            buildings.Add(building);
            world.RedrawAction?.Invoke();
        }

        public void RemoveBuilding(Building building)
        {
            buildings.Remove(building);
            world.RedrawAction?.Invoke();
        }

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
            unit.name = $"{color}{unit.unitType.name}{units.Count}";
            world.RedrawAction?.Invoke();
        }

        public void RemoveUnit(Unit unit)
        {
            units.Remove(unit);
            world.RedrawAction?.Invoke();
        }

		public override void SerializeFields()
		{
            SerializeField(name);
            SerializeField(color);

            SerializeField<Unit>(units);
            SerializeField<Building>(buildings);
            SerializeField(territory.Count); //Save only the coords.
            foreach(TileMap.Tile tile in territory)
            {
                SerializeField(tile.pos);
            }
             
		}

		public override void DeserializeFields()
		{
            name = DeserializeField<string>(); 
            color = DeserializeField<string>();

            units = DeserializeList<Unit>((Unit u) =>
            {
                u.tileMap = world.map;
                u.nation = this;
            });
            buildings = DeserializeList<Building>((Building b) =>
			{
				b.tileMap = world.map;
                b.nation = this;
			});

            int territoryCount = DeserializeField<int>();
            for(int i = 0; i < territoryCount; i++)
            {
                AddTerritory(world.map.getTile(DeserializeField<Vector2Int>()));
            }
            
		}
	}
}
