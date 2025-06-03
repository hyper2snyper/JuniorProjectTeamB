using JuniorProject.Backend.Agents.Objectives;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Frontend.Components;
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
        public int GetUnitCap { get
            {
                int cap = 0;
                foreach(Building building in buildings)
                {
                    if(building.template.flags.HasFlag(Building.BuildingTemplate.BuildingFlags.GARRISON))
                    {
                        cap += building.template.unitCap;
                    }
                }
                return cap;
            } }

        public Building? capital;
        public List<Building> buildings = new List<Building>();
        public List<TileMap.Tile> territory = new List<TileMap.Tile>();
		public Stack<Tile> desiredTerritory = new Stack<Tile>();


		public Dictionary<string, int> resources = new Dictionary<string, int>();
        Dictionary<string, int> netChange = new Dictionary<string, int>();

        const ulong CALCULATION_INTERVAL = 10;
        Timer<ulong> calculationTimer = new Timer<ulong>(CALCULATION_INTERVAL+1, CALCULATION_INTERVAL);
        const ulong STATE_CHANGE_INTERVAL = 10;
        Timer<ulong> stateChangeTimer = new Timer<ulong>(0, STATE_CHANGE_INTERVAL);

        enum AIState
        { 
            EXPAND,
            DEVELOP,
            PREPARE,
            WAR,
        }
        AIState state = AIState.EXPAND;
        Nation? opponent = null; //Currently competing against.

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
            foreach(TileMap.Tile neighbor in world.map.getTileNeighbors(tile, (TileMap.Tile t) => t.impassible))
            {
                if (neighbor == null) continue;
                AddTerritory(neighbor);
            }
            Building castle = new Building("Capital", world.map, tile, this);
            AddBuilding(castle);
            capital = castle;
            Unit unit = new Unit("Builder", $"builder{color}", this, world.map, tile);
            AddUnit(unit);
            Unit soldier = new Unit("Soldier", $"soldier{color}", this, world.map, tile);
            AddUnit(soldier);
        }

        public void CalculateState()
        {
            int totalDeficit = 0;
            foreach(string resource in netChange.Keys)
            {
                if (netChange[resource] > 0) continue;
                totalDeficit += netChange[resource];
            }
            if(totalDeficit < -5)
            {
                state = AIState.DEVELOP;
                return;
            }
            state = AIState.EXPAND;

        }

        public void CalculateObjectives()
        {
            switch(state)
            {
                case AIState.DEVELOP:
                    {
                        foreach(Unit unit in units)
                        {
                            if (!unit.unitType.flags.HasFlag(Unit.UnitTemplate.UnitFlags.BUILDER)) continue;
                            if (unit.GetObjective() != null) continue;
                            string resourceToExpand = "";
                            int smallest = int.MaxValue;
                            foreach(string resource in netChange.Keys)
                            {
                                if (netChange[resource] < smallest)
                                {
                                    smallest = netChange[resource];
                                    resourceToExpand = resource;
                                }
                            }
                            List<Tile> expandLocations = appraiseSurroundings(10, (Tile t) =>
                            {
                                return t.tileResources[resourceToExpand] - (t.pos - capital.PosVector).Magnitude;
                            });
                            Tile tileToExpand = expandLocations[0];
                            unit.SetObjective(new MoveAction(tileToExpand, (Tile t, Unit u) =>
                            {
                                if (resourceToExpand == "Food" || resourceToExpand == "Wood")
                                {
                                    CheckToAddBuilding("Farm", t);
                                }
                                if (resourceToExpand == "Iron" || resourceToExpand == "Gold" || resourceToExpand == "Stone")
                                {
                                    CheckToAddBuilding("Mine", t);
                                }
                                return null;
                            }, (Tile t, Unit u, bool nopath) => {
                                if (!nopath) return null;
                                List<Tile> portLocations = appraiseSurroundings(20, (Tile t) =>
                                {
                                    return (t.coast ? 100 : -100) - (t.pos - capital.PosVector).Magnitude;
                                });
                                return new MoveAction((portLocations[0]), (Tile t, Unit u) =>
                                {
                                    CheckToAddBuilding("Port", t);
                                    return null;
                                });
                            }));
                        }
                        break;
                    }
                case AIState.EXPAND:
                    {
                        Objective? PostMove(Tile t, Unit u)
                        {
                            if (desiredTerritory.Count == 0) return null;
                            Tile next = desiredTerritory.Pop();
                            if (next == null) return null;
                            u.MoveTo(next, PostMove);
                            return u.GetObjective();
                        }
                        if (desiredTerritory.Count == 0) desiredTerritory = new Stack<Tile>(GetBorderingTiles());
						foreach (Unit unit in units)
                        {
                            if (!unit.unitType.flags.HasFlag(Unit.UnitTemplate.UnitFlags.WARRIOR)) continue;
                            if (desiredTerritory.Count == 0) break;
							Tile t = desiredTerritory.Pop();
                            if(t == null) continue;
                            unit.MoveTo(t, PostMove);
                        }
                        break;
                    }
                case AIState.PREPARE:
                    {
                        break;
                    }
                case AIState.WAR:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void TakeTurn(ulong tick)
        {
            if (calculationTimer.Tick(tick))
            {
                Debug.Print("Calculating Objectives");
                if(stateChangeTimer.Tick())
                {
                    Debug.Print("Calculating State");
                    CalculateState();               
                }
                CalculateObjectives();
            }
            Dictionary<string, int> prev = new Dictionary<string, int>(resources);
            foreach (Building building in buildings)
            {
                building.TakeTurn(tick);
            }
            foreach (Unit unit in units)
            {
                unit.TakeTurn(tick);
            }
            foreach(string resource in resources.Keys)
            {
                netChange[resource] = resources[resource] - prev[resource];
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

        public delegate int appraisalDelegate(Tile t);
        public List<Tile> appraiseSurroundings(int radius, appraisalDelegate appraisal, bool isempty = true)
        {
            SortedSet<Tile> sorted = new SortedSet<Tile>( Comparer<Tile>.Create((Tile first, Tile second) =>
            {
                return Math.Clamp(appraisal(second) - appraisal(first), -1, 1);
            }));
            for(int x = Math.Clamp(capital.pos.pos.X-radius, 0, world.map.mapSize.X); x < capital.pos.pos.X+radius; x++)
            {
                for(int y = Math.Clamp(capital.pos.pos.Y-radius, 0, world.map.mapSize.Y); y < capital.pos.pos.Y+radius; y++)
                {
                    Tile? t = world.map.getTile(new Vector2Int(x, y));
                    if (t == null) continue;
                    if(isempty)
                    {
                        if (t.HasBuilding) continue;
                    }
                    if(t.impassible) continue;
                    sorted.Add(t);
                }
            }
            return sorted.ToList();
        }

		public List<TileMap.Tile> GetBorderingTiles()
		{
			List<TileMap.Tile> borderingTiles = new List<TileMap.Tile>();
            foreach(Tile t in territory)
            {
                foreach (Tile neighbor in world.map.getPassableTileNeighbors(t))
                {
                    if (borderingTiles.Contains(neighbor)) continue;
                    if(territory.Contains(neighbor)) continue;
                    borderingTiles.Add(neighbor);
                }
            }
			return borderingTiles;
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
       

        public void AddTerritory(TileMap.Tile tile)
        {
            tile.Owner = this;
            territory.Add(tile);
        }
        public void RemoveTerritory(TileMap.Tile tile)
        {
            territory.Remove(tile);
        }

        public void AddBuilding(Building building)
        {
            if(building.pos.Owner != this)
            {
                AddTerritory(building.pos);
            }
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
            Building.BuildingTemplate buildingType = Building.buildingTemplates[type];
            if (resources["Gold"] >= buildingType.cost)
            {
                resources["Gold"] -= buildingType.cost;
                AddBuilding(new Building(type, world.map, tile, this));
            }
        }

        public void CheckToAddUnit(string type)
        {
            if (resources["Iron"] >= Unit.unitTemplates[type].ironCost)
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
            SerializeField<string, int>(netChange);
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
            netChange = DeserializeDictionary<string, int>();
        }
    }
}
