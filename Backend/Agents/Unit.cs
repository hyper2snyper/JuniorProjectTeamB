using System.IO;
using System.Data.SQLite;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System.Security.Policy;
using System.Xml.Linq;
using JuniorProject.Backend.Agents.Objectives;

namespace JuniorProject.Backend.Agents
{
    public class Unit : Mob
    {
        public class UnitTemplate //The template of the unit, the type if you will.
        {
            public string name;
            public string description;
            public int attackDamage;
            public int attackRange;
            public int maxHealth;
            public string sprite;
            public int flags;
        }
        public static Dictionary<string, UnitTemplate> unitTemplates;
        public static void LoadUnitTemplates()
        {
            unitTemplates = new Dictionary<string, UnitTemplate>();
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Units;");
            while (results.Read())
            {
                UnitTemplate template = new UnitTemplate();
                template.name = results.GetString(0);
                template.description = results.GetString(1);
                template.attackDamage = results.GetInt32(2);
                template.attackRange = results.GetInt32(3);
                template.maxHealth = results.GetInt32(4);
                template.sprite = results.GetString(5);
                template.flags = results.GetInt32(6); //PLACEHOLDER
                unitTemplates.Add(template.name, template);
            }
        }


        public UnitTemplate unitType;

        public string name;
        public int health;
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
        }

		void SetType(UnitTemplate template)
		{
			unitType = template;
            sprite = unitType.sprite;

			health = unitType.maxHealth;
		}

		public override string GetSprite()
		{
			return $"{nation?.color}{base.GetSprite()}";
		}

        public void SetObjective(Objective objective)
        {
            this.objective = objective;
            objective.Attach(this);
        }

        public override void TakeTurn(ulong tick)
        {
            base.TakeTurn(tick);
            if(objective != null)
            {
                objective = objective.PerformTurn(tick);
            }
        }

        public void TakeDamage(int damage)
        {

        }


		public override void EnterTile(TileMap.Tile tile)
		{
            if (tile.Owner != nation)
            {
                nation.AddTerritory(tile);
            }
			base.EnterTile(tile);
		}


		public void MoveTo(TileMap.Tile toPos)
        {
            SetObjective(new MoveAction(toPos));
        }

        public void Follow(Unit mob) { 
            SetObjective(new FollowAction(mob));
        }

        public bool IsAdjacent(Vector2Int pos) {
            return false;
        }

        public override void SerializeFields()
        {
            base.SerializeFields();
            SerializeField(unitType.name); //Save the type.
            SerializeField(health);
            SerializeField(name);

            //Eventually we need to save objective states.
        }

        public override void DeserializeFields()
        {
            base.DeserializeFields();
            unitType = unitTemplates[DeserializeField<string>()]; //The templates need to be loaded first.
            health = DeserializeField<int>();
            name = DeserializeField<string>();
            
        }
    }
}