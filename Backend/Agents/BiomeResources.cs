using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using static JuniorProject.Backend.Agents.BiomeResources;
using System.Xml.Linq;
using static JuniorProject.Backend.Agents.Building;

namespace JuniorProject.Backend.Agents
{
    public class BiomeResources : Mob
    {
        public class BiomeResourcesTemplate
        {
            public string Name;
            public string Resources;
            public int GatherRate;

            public BiomeResourcesTemplate()
            {
                SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM BiomeResource;");
                while (results.Read())
                {
                    Name = results.GetString(0);
                    Resources = results.GetString(1);
                    GatherRate = results.GetInt32(2);
                }
            }
        }

        public static Dictionary<(string Biome, string Resource), BiomeResourcesTemplate> biomeResourcesTemplate = new();

        public static void LoadBiomeResourcesTemplate()
        {
            biomeResourcesTemplate.Clear();

            using var results = DatabaseManager.ReadDB("SELECT * FROM BiomeResource;");
            while (results.Read())
            {
                var biomeID = results.GetString(0);    // BiomeID
                var resourceID = results.GetString(1); // ResourceID
                var gatherRate = results.GetInt32(2);  // GatherRate

                var key = (biomeID, resourceID);

                biomeResourcesTemplate[key] = new BiomeResourcesTemplate
                {
                    Name = biomeID,
                    Resources = resourceID,
                    GatherRate = gatherRate
                };
            }
        }



        public static void SaveBiomeResourcesTemplate()
        {
            foreach (var template in biomeResourcesTemplate.Values)
            {
                DatabaseManager.WriteDB(
                    "UPDATE BiomeResource SET GatherRate=@GatherRate WHERE ResourceID=@Resources AND BiomeID=@Name",
                    new Dictionary<string, object>
                    {
                        { "@GatherRate", template.GatherRate },
                        { "@Resources", template.Resources },
                        { "@Name", template.Name }
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
