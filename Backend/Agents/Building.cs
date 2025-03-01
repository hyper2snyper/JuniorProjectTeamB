using JuniorProject.Backend.Helpers;
using System;
using System.Data.SQLite;


namespace JuniorProject.Backend.Agents
{
    class Building : Serializable
    {

        public class BuildingTemplate
        {
            public string name;
            public int cost;
            public int maxHealth;
            public string sprite;
        }
        public static Dictionary<string, BuildingTemplate> buildingTemplates;
        public static BuildingTemplate capitalTemplate;
        public static void LoadBuildingTemplates()
        {
            buildingTemplates = new Dictionary<string, BuildingTemplate>();
			SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
			while (results.Read())
			{
                BuildingTemplate template = new BuildingTemplate();
				template.name = results.GetString(0);
				template.cost = results.GetInt32(1);
                template.maxHealth = results.GetInt32(2);
				template.sprite = results.GetString(3);
				if (results.GetInt32(4) != 0)
                {
                    capitalTemplate = template;
                }
                buildingTemplates.Add(template.name, template);
			}
		}


        Nation owner;
        int health;

        public Vector2Int gridPosition;
        public string sprite;

        public Building(string type, Nation nation, Vector2Int posV)
        {
            owner = nation;
            gridPosition = posV;

            switch (type) {
                case "WheatFarm":
                    sprite = type;
                    break;
                case "Mine":
                    sprite = type;
                    break;
                case "Castle":
                    sprite = $"{owner.color}{type}";
                    break;
                case "House":
                    sprite = $"{owner.color}{type}";
                    break;
                default:
                    Debug.Print($"ERROR!!! Given building type \"{type}\" is not resolvable");
                    break;
            }
        }

        public override void DeserializeFields()
        {
            throw new NotImplementedException();
        }

        public override void SerializeFields()
        {
            throw new NotImplementedException();
        }




    }
}