using System.IO;
using System.Data.SQLite;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System.Security.Policy;
using System.Xml.Linq;
using JuniorProject.Backend.Agents.Objectives;

namespace JuniorProject.Backend.Agents
{
    public class Unit : Serializable
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
        public int health;
        public string team;

        Objective? objective;

        public TileMap tileMap;
        TileMap.Tile pos;

        public Unit() { }
        public Unit(string type, string team, World world, Vector2Int posV)
        {
            tileMap = world.map;
            this.team = team;
            pos = tileMap.getTile(posV);
            if (!unitTemplates.Keys.Contains(type))
            {
                Debug.Print($"Unit was attempted to be created with type {type}, but that type does not exist.");
                return;
            }
            SetType(unitTemplates[type]);
        }

        public string getSpriteName()
        {
            return String.Format("{0:S}{1:S}", team, unitType.name);
        }

        public TileMap.Tile getPosition()
        {
            return pos;
        }

        public bool tryEnter(TileMap.Tile toEnter)
        {
            return true;
        }

        public void setPosition(TileMap.Tile newPos)
        {
            
            pos = newPos;
			tileMap.TilesUpdated();

		}

        void SetType(UnitTemplate template)
        {
            unitType = template;
            health = template.maxHealth;
        }

        public void SetObjective(Objective objective)
        {
            this.objective = objective;
            objective.Attach(this);
        }

        public void TakeTurn()
        {
            if(objective != null)
            {
                objective = objective.PerformTurn();
            }
        }

        public void TakeDamage(int damage)
        {

        }

        public void MoveTo(TileMap.Tile toPos)
        {
            SetObjective(new MoveAction(toPos));
        }

        public void MoveTo(Vector2Int toPos)
        {
            MoveTo(tileMap.getTile(toPos));
        }


        public override void SerializeFields()
        {
            SerializeField(unitType.name); //Save the type.
            SerializeField(health);
            SerializeField(team);

            //Eventually we need to save objective states.
        }

        public override void DeserializeFields()
        {
            unitType = unitTemplates[DeserializeField<string>()]; //The templates need to be loaded first.
            health = DeserializeField<int>();
            team = DeserializeField<string>();

            //pos will be set in a linking step after load.
        }
    }
}