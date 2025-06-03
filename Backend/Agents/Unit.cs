using System.IO;
using System.Data.SQLite;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System.Security.Policy;
using System.Xml.Linq;
using JuniorProject.Backend.Agents.Objectives;
using JuniorProject.Frontend.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JuniorProject.Backend.Agents
{
    public class Unit : Mob
    {

        public class UnitTemplate
        {
            [JsonPropertyName("UnitType")]
            public string name { get; set; }

            [JsonPropertyName("AttackDamage")]
            public int attackDamage { get; set; }

            [JsonPropertyName("AttackRange")]
            public int attackRange { get; set; }

            [JsonPropertyName("MaxHealth")]
            public int maxHealth { get; set; }

            [JsonPropertyName("Sprite")]
            public string sprite { get; set; }


            public enum UnitFlags
            {
                NONE = 0,
                BUILDER = 1 << 1,
            }
            [JsonPropertyName("Flags")]
            public int flags {  get; set; }
            [JsonPropertyName("IronCost")]
            public int ironCost { get; set; }
            [JsonPropertyName("FoodCost")]
            public int foodCost { get; set; }
        }

        public static Dictionary<string, UnitTemplate> unitTemplates;
        public static void LoadUnitTemplates()
        {
            if (unitTemplates != null && unitTemplates.Count > 0)
                return;

            unitTemplates = new Dictionary<string, UnitTemplate>();
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Units;");
            while (results.Read())
            {
                UnitTemplate template = new UnitTemplate();
                template.name = results.GetString(0);
                template.attackDamage = results.GetInt32(2);
                template.attackRange = results.GetInt32(3);
                template.maxHealth = results.GetInt32(4);
                template.sprite = results.GetString(5);
                template.flags = results.GetInt32(6);
                template.ironCost = results.GetInt32(7);
                template.foodCost = results.GetInt32(8);
                unitTemplates.Add(template.name, template);
            }
        }

        public static void SaveAllUnitTemplates()
        {
            foreach (var template in unitTemplates.Values)
            {
                DatabaseManager.WriteDB(
                    "UPDATE Units SET AttackDamage=@dmg, AttackRange=@rng, MaxHealth=@hp, IronCost=@ironcost, FoodCost=@foodcost WHERE UnitType=@name",
                    new Dictionary<string, object>
                    {
                        {"@dmg", template.attackDamage},
                        {"@rng", template.attackRange},
                        {"@hp", template.maxHealth},
                        {"@name", template.name},
                        {"@sprite", template.sprite },
                        { "@ironcost", template.ironCost},
                        { "@foodcost", template.foodCost }
                    }
                );
            }
        }

        public static void ResetUnitTemplatesFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Default data JSON not found: {jsonFilePath}");

            string json = File.ReadAllText(jsonFilePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<UnitTemplate>>>(json);

            if (jsonData != null && jsonData.ContainsKey("Units"))
            {
                foreach (var unit in jsonData["Units"])
                {
                    if (string.IsNullOrWhiteSpace(unit.name)) continue;

                    DatabaseManager.WriteDB(
                        "INSERT OR REPLACE INTO Units (UnitType, AttackDamage, AttackRange, MaxHealth, Sprite, Flags, IronCost, FoodCost) " +
                        "VALUES (@name, @dmg, @range, @health, @sprite, @flag, @ironcost, @foodcost)",
                        new Dictionary<string, object>
                        {
                            { "@name", unit.name },
                            { "@dmg", unit.attackDamage },
                            { "@range", unit.attackRange },
                            { "@health", unit.maxHealth },
                            { "@sprite", unit.sprite},
                            { "@flag", "0"},
                            { "@ironcost", unit.ironCost },
                            { "@foodcost", unit.foodCost }
                        });


                }

                unitTemplates = null;
                LoadUnitTemplates();
            }
        }



        public UnitTemplate unitType;

        public string name;
        public int health;
        public bool embarked = false;
        public Vector2Int followObjectivePosition;

        Objective? objective;

        public Unit() { }

        public Unit(string type, string name, Nation? nation, TileMap tileMap, TileMap.Tile tile) : base(tileMap, tile, nation)
        {
            this.name = name;
            if (!unitTemplates.Keys.Contains(type))
            {
                Debug.Print($"Unit was attempted to be created with type {type}, but that type does not exist.");
                return;
            }
            SetType(unitTemplates[type]);
            drawableType = GenericDrawable.DrawableType.Unit;
        }

        void SetType(UnitTemplate template)
        {
            unitType = template;
            health = unitType.maxHealth;
            sprite = unitType.sprite;
        }

        public override string GetSprite()
        {
            return $"{nation?.color}{(embarked ? "Ship" : base.GetSprite())}";
        }

        public void SetObjective(Objective objective)
        {
            this.objective = objective;
            objective.Attach(this);
        }

        public Objective? GetObjective() { return objective; }

        public override void TakeTurn(ulong tick)
        {
            if (nation.resources["Food"] >= unitType.foodCost)
            {
                Debug.Print($"Foodcost: {unitType.foodCost}");
                nation.resources["Food"] -= unitType.foodCost;
                base.TakeTurn(tick);
                if (objective != null)
                {
                    objective = objective.PerformTurn(tick);
                }
            }
        }

        public void TakeDamage(int damage)
        {

        }

		public override bool TryEnter(TileMap.Tile tile)
		{
            if (tile.Occupants.Count == 0) return !tile.impassible;
            if(pos.coast && tile.impassible)
            {
				foreach (Mob mob in pos.Occupants)
				{
					if (mob is not Building) continue;
					Building building = (Building)mob;
					if (building.template.name != "Port") continue;
                    return true;
			    }
                return false;
			}
            return !tile.impassible;
		}


		public override void EnterTile(TileMap.Tile tile)
        {
            foreach (Mob m in tile.Occupants)
            {
                if (m is Building b)
                {
                    if (m.nation == nation) continue;
                    nation?.AddBuilding(b);
                }
            }
            if (tile.impassible) embarked = true;
            if (embarked && !tile.impassible) embarked = false;
            base.EnterTile(tile);
            
        }

        public void MoveTo(TileMap.Tile toPos, MoveAction.PostMoveAction? action = null)
        {
            SetObjective(new MoveAction(toPos, action));
        }

        public void Follow(Unit mob)
        {
            SetObjective(new FollowAction(mob));
        }

        public override void populateDrawables(ref List<GenericDrawable> genericDrawables)
        {
            genericDrawables.Add(new GenericDrawable(PosVector, GetSprite(), GenericDrawable.DrawableType.Unit, name));
        }

        public override void DestroyMob()
        {
            base.DestroyMob();
            objective = null;
        }

        public override void SerializeFields()
        {
            base.SerializeFields();
            SerializeField(unitType.name);
            SerializeField(health);
            SerializeField(name);
        }

        public override void DeserializeFields()
        {
            base.DeserializeFields();
            unitType = unitTemplates[DeserializeField<string>()];
            health = DeserializeField<int>();
            name = DeserializeField<string>();

        }
    }
}