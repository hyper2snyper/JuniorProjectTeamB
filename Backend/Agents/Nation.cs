using JuniorProject.Backend.Agents.Objectives;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Frontend.Components;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using static JuniorProject.Backend.WorldData.EconomyManager;
using static JuniorProject.Backend.WorldData.TileMap;

namespace JuniorProject.Backend.Agents
{
    public class Nation : Serializable
    {
        public string name = "";
        public string color = "";

        World world;
        public World World { set { world = value; } }

        public List<Unit> units = new List<Unit>();

        public Building? capital;
        public List<Building> buildings = new List<Building>();
        public List<TileMap.Tile> territory = new List<TileMap.Tile>();
        public Dictionary<string, int> resources = new Dictionary<string, int>();

        public List<TileMap.Tile> desiredLand = new List<TileMap.Tile>();

        Timer<ulong> calculationTimer = new Timer<ulong>(0, 10);

        public int maxUnits = 3;

        public List<Mob> mobsToRemove = new List<Mob>();

        public Nation() { }
        public Nation(string name, string color, World world)
        {
            this.name = name;
            this.color = color;
            this.world = world;
        }

        public Nation(string name, string color, int quadrant, World world) : this(name, color, world)
        {
            SQLiteDataReader resourceResults = DatabaseManager.ReadDB("SELECT * FROM Resources;");
            while (resourceResults.Read())
            {
                resources[resourceResults.GetString(0)] = resourceResults.GetInt32(1);
            }

            PlaceStart(quadrant);
        }

        public void PlaceStart(int quadrant)
        {
            Vector2Int startingPos = new Vector2Int();
            switch (quadrant)
            {
                case 0: startingPos = new Vector2Int((world.map.mapSize.X / 4), (world.map.mapSize.Y / 4)); break;
                case 1: startingPos = new Vector2Int((world.map.mapSize.X / 2) + (world.map.mapSize.X / 4), (world.map.mapSize.Y / 4)); break;
                case 2: startingPos = new Vector2Int((world.map.mapSize.X / 4), (world.map.mapSize.Y / 2) + (world.map.mapSize.Y / 4)); break;
                case 3: startingPos = new Vector2Int((world.map.mapSize.X / 2) + (world.map.mapSize.X / 4), (world.map.mapSize.Y / 2) + (world.map.mapSize.Y / 4)); break;
            }

            TileMap.Tile tile = world.map.getTile(startingPos);
            for (int x = -1; x < 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    for (int i = 1; tile == null || tile.impassible; i++)
                    {
                        if (startingPos.X + (x * i) > world.map.mapSize.X || startingPos.X + (x * i) < 0) break;
                        if (startingPos.Y + (y * i) > world.map.mapSize.Y || startingPos.Y + (y * i) < 0) break;
                        tile = world.map.getTile(new Vector2Int(startingPos.X + (x * i), startingPos.Y + (y * i)));
                    }
                    if (tile != null && tile.impassible == false) break;
                }
                if (tile != null && tile.impassible == false) break;
            }
            AddTerritory(tile);
            Building castle = new Building("Capital", world.map, tile, this);
            AddBuilding(castle);
            capital = castle;
            Unit unit = new Unit("Soldier", $"soldier{color}", this, world.map, tile);
            AddUnit(unit);
        }

        public Objective? UnitPostMove(TileMap.Tile tile, Unit unit)
        {
            Debug.Print($"{tile} has been claimed for team {name}");
            if (desiredLand.Count == 0) return null;

            if (tile.Occupants.Count <= 1)
            {
                if (tile.terrainPercentages.ContainsKey("Grassland") && tile.terrainPercentages["Grassland"] >= 0.8f )
                {
                    CheckToAddBuilding("Farm", tile);
                }
            }

            MoveAction moveAction = new MoveAction(desiredLand[0], UnitPostMove);
            desiredLand.Remove(desiredLand[0]);
            moveAction.Attach(unit);
            return moveAction;
        }

        public void CalculateObjectives()
        {
            List<Unit> unassignedUnits = new List<Unit>();
            foreach (Unit unit in units)
            {
                if (unit.GetObjective() != null) continue;
                unassignedUnits.Add(unit);
            }
            if (unassignedUnits.Count < desiredLand.Count) return;
            desiredLand = GetBorderingTiles(
                (TileMap.Tile x, TileMap.Tile y) =>
                {
                    if (x.Occupants.Count > y.Occupants.Count) return 10;
                    if (x.terrainPercentages.ContainsKey("Grassland"))
                    {
                        if (y.terrainPercentages.ContainsKey("Grassland"))
                        {
                            return (x.terrainPercentages["Grassland"] > y.terrainPercentages["Grassland"]) ? 50 : -50;
                        }
                        return 1;
                    }
                    if (x.movementCost == y.movementCost) return 0;
                    return (x.movementCost > y.movementCost ? 1 : -1);
                });
            if (desiredLand.Count == 0)
            {
                Building? port = null;
                foreach (Building building in buildings)
                {
                    if (building.template.name != "Port") continue;
                    port = building;
                    break;
                }
                if (port == null)
                {
                    TileMap.Tile? shoreTile = null;
                    foreach (TileMap.Tile tile in territory)
                    {
                        if (!tile.coast || tile.Occupants.Count >= 1) continue;
                        shoreTile = tile;
                        break;
                    }
                    if (shoreTile == null) return; //Sucks to suck i guess.
                    if (unassignedUnits.Count == 0) return;
                    unassignedUnits[0].MoveTo(shoreTile,
                        (TileMap.Tile tile, Unit unit) =>
                        {
                            AddBuilding(new Building("Port", world.map, shoreTile, this));
                            return null;
                        });
                    return;
                }
                foreach (Unit unit in unassignedUnits)
                {
                    unit.MoveTo(port.pos,
                        (TileMap.Tile tile, Unit u) =>
                        {
                            TileMap.Tile? nearestOcean = null;
                            foreach (TileMap.Tile neighbor in world.map.getTileNeighbors(tile,
                                (TileMap.Tile t) =>
                                {
                                    return !t.impassible;
                                }))
                            {
                                nearestOcean = neighbor;
                                if (nearestOcean == null) continue;
                                Vector2Int dir = (nearestOcean.pos - tile.pos).Normalize;
                                TileMap.Tile? currentTile = nearestOcean;
                                while (nearestOcean.impassible)
                                {
                                    currentTile = world.map.getTile(currentTile.pos + dir);
                                    if (currentTile == null) break;
                                    if (!currentTile.impassible) break;
                                }
                                if (currentTile == null) continue;
                                Sail sail = new Sail(currentTile);
                                sail.Attach(u);
                                return sail;
                            }
                            return null;
                        });
                }
                return;
            }
            foreach (Unit unit in unassignedUnits)
            {
                unit.MoveTo(desiredLand[0], UnitPostMove);
                desiredLand.Remove(desiredLand[0]);
                if (desiredLand.Count == 0) break;
            }
        }

        public void TakeTurn(ulong tick)
        {
            if (calculationTimer.Tick(tick))
            {
                Debug.Print("Calculating Objectives");
                CalculateObjectives();
                CheckToAddUnit("Soldier");
            }
            foreach (Building building in buildings)
            {
                building.TakeTurn(tick);
            }

            foreach (Unit unit in units)
            {
                unit.TakeTurn(tick);
            }

            foreach (Mob mob in mobsToRemove)
            {
                if (mob is Unit u)
                {
                    units.Remove(u);
                }
                if (mob is Building b)
                {
                    buildings.Remove(b);
                }
            }
            mobsToRemove.Clear();
        }

        public void PopulateDrawablesList(ref List<GenericDrawable> genericDrawables)
        {
            foreach (TileMap.Tile tile in territory)
            {
                genericDrawables.Add(new GenericDrawable(tile.pos, $"{color}TileCover", GenericDrawable.DrawableType.Tile, $"{tile.pos.X}-{tile.pos.Y}"));
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

        public delegate int appraisalDelegate(TileMap.Tile t1, TileMap.Tile t2);
        public List<TileMap.Tile> GetBorderingTiles(appraisalDelegate? appraisal = null)
        {
            List<TileMap.Tile> borderingTiles = new List<TileMap.Tile>();

            foreach (TileMap.Tile tile in territory)
            {
                foreach (TileMap.Tile? possibleTile in world.map.getPassableTileNeighbors(tile))
                {
                    if (possibleTile == null) continue;
                    if (possibleTile.Owner == this) continue;
                    if (borderingTiles.Contains(possibleTile)) continue;
                    borderingTiles.Add(possibleTile);
                }
            }
            if (appraisal != null)
            {
                borderingTiles.Sort(new Comparison<TileMap.Tile>(appraisal));
            }
            return borderingTiles;
        }

        public void AddTerritory(TileMap.Tile tile)
        {
            tile.Owner = this;
            territory.Add(tile);
            if (desiredLand.Contains(tile))
                desiredLand.Remove(tile);
        }

        public void RemoveTerritory(TileMap.Tile tile)
        {
            territory.Remove(tile);
        }

        public void AddBuilding(Building building)
        {
            if (building.nation != null)
            {
                building.nation.RemoveBuilding(building);
            }
            buildings.Add(building);
            building.nation = this;
        }

        public void RemoveBuilding(Building building)
        {
            buildings.Remove(building);
            if (building == capital)
            {
                DeleteNation();
            }
        }

        public void CheckToAddBuilding(string type, Tile tile)
        {
            using (var reader = DatabaseManager.ReadDB($"SELECT BuildingCost FROM Buildings WHERE BuildingName = '{type}'"))
            {
                if (reader.Read())
                {
                    int buildingCost = Convert.ToInt32(reader["BuildingCost"]);
                    if (resources["Wood"] > buildingCost)
                    {
                        resources["Wood"] -= buildingCost;
                        AddBuilding(new Building("Farm", world.map, tile, this));
                    }
                }
            }
        }

        public void CheckToAddUnit(string type)
        {
            if (resources["Iron"] >= Unit.unitTemplates[type].ironCost && units.Count < maxUnits)
            {
                resources["Iron"] -= Unit.unitTemplates[type].ironCost;
                AddUnit(new Unit(type, "", this, world.map, capital.pos));
            }
        }

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
            unit.name = $"{color}{unit.unitType.name}{units.Count}";
        }

        public void RemoveUnit(Unit unit)
        {
            units.Remove(unit);
            unit.nation = null;
        }



        public void DeleteNation()
        {
            foreach (Unit unit in units)
            {
                unit.DestroyMob();
            }
            units.Clear();
            territory.Clear();
            desiredLand.Clear();
            world.nations.Remove(color);

            foreach (var r in resources)
            {
                world.economyManager.demands.Remove($"{color}-{r.Key}");
            }
            world.economyManager.potentialTrades.RemoveAll((Trade t) => t.initiator == color);
        }

        public override void SerializeFields()
        {
            SerializeField(name);
            SerializeField(color);

            SerializeField<Unit>(units);
            SerializeField<Building>(buildings);
            SerializeField(territory.Count); //Save only the coords.
            foreach (TileMap.Tile tile in territory)
            {
                SerializeField(tile.pos);
            }
            SerializeField<string, int>(resources);
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
            capital = buildings[0];
            int territoryCount = DeserializeField<int>();
            for (int i = 0; i < territoryCount; i++)
            {
                AddTerritory(world.map.getTile(DeserializeField<Vector2Int>()));
            }
            resources = DeserializeDictionary<string, int>();
        }
    }
}
