using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JuniorProject.Backend.Agents.Building;
using System.Security.Policy;

namespace JuniorProject.Backend.Agents
{
    public class Building : Serializable
    {

        public class BuildingTemplate
        {
            public string name;
            public int cost;
            //public int maxHealth;
            //public int sprite;
            public BuildingTemplate()
            {
                SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
                while (results.Read())
                {
                    name = results.GetString(0);
                    cost = results.GetInt32(1);
                    //sprite = results.GetInt32(3);
                }
            }

        }

        Nation owner;
        int health;

        public static Dictionary<string, BuildingTemplate> buildingTemplates = new();
        public static void LoadBuildingTemplates()
        {
            buildingTemplates.Clear();

            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
            while (results.Read())
            {
                var template = new BuildingTemplate
                {
                    name = results.GetString(0),
                    cost = results.GetInt32(1),
                    //sprite = results.GetInt32(3)
                };

                buildingTemplates[template.name] = template;
            }
        }



        public static void SaveBuildingTemplates()
        {
            foreach (var template in buildingTemplates.Values)
            {
                DatabaseManager.WriteDB(
                    "UPDATE Buildings SET BuildingCost=@cost WHERE BuildingName=@name",
                    new Dictionary<string, object>
                    {
                        { "@cost", template.cost },
                        { "@name", template.name }
                    }
                );
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