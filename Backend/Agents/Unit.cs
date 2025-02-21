using System.IO;
using System.Data.SQLite;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System.Security.Policy;
using System.Xml.Linq;

namespace JuniorProject.Backend.Agents
{
    class Unit : Serializable
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

            public UnitTemplate()
            {
                SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Units;");
                while (results.Read())
                {
                    name = results.GetString(0);
                    description = results.GetString(1);
                    attackDamage = results.GetInt32(2);
                    attackRange = results.GetInt32(3);
                    maxHealth = results.GetInt32(4);
                    sprite = results.GetString(5);
                    flags = results.GetInt32(6);
                }
            }
        }
        static Dictionary<string, UnitTemplate> unitTemplates;
        public static void LoadUnitTemplates()
        {
			unitTemplates = new Dictionary<string, UnitTemplate>();
			UnitTemplate template = new UnitTemplate();
			SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Units;");
			while (results.Read())
			{
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

        IObjective? objective;

        TileMap tileMap;
        TileMap.Tile pos;
        Vector2Int posV;

        public Unit(string type, World world, Vector2Int posV)
        {
            this.posV = posV;
            tileMap = world.map;
            pos = tileMap.getTile(posV);
            if(!unitTemplates.Keys.Contains(type))
            {
                Debug.Print($"Unit was attempted to be created with type {type}, but that type does not exist.");
                return;
            }
            SetType(unitTemplates[type]);
        }

        void SetType(UnitTemplate template)
        {
            unitType = template;
            health = template.maxHealth;
        }

        public void SetObjective(IObjective objective)
        {
            this.objective = objective;
        }

        public void TakeTurn()
        {

        }

        public void TakeDamage(int damage)
        {

        }

        public void MoveTo(TileMap.Tile toPos)
        {
			List<TileMap.Tile> pathway = Astar.FindPath(tileMap, pos, toPos, (TileMap.Tile tile, TileMap.Tile target) =>
            {
                return (target.pos-tile.pos).Magnitude;
            });

        }

        public void MoveTo(Vector2Int toPos)
        {
            MoveTo(tileMap.getTile(toPos));
        }


        public override void SerializeFields()
        {

        }

        public override void DeserializeFields()
        {

        }
    }
}