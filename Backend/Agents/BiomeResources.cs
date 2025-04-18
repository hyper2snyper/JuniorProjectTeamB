using System.Data.SQLite;
using System.Text.Json;
using System.IO;
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
            if (biomeResourcesTemplate == null)
                biomeResourcesTemplate = new Dictionary<(string Biome, string Resource), BiomeResourcesTemplate>();
            else if (biomeResourcesTemplate.Count > 0)
                return;

            biomeResourcesTemplate.Clear(); 

            using var results = DatabaseManager.ReadDB("SELECT * FROM BiomeResource;");
            while (results.Read())
            {
                var biomeID = results.GetString(0);    
                var resourceID = results.GetString(1); 
                var gatherRate = results.GetInt32(2);  

                var key = (biomeID, resourceID);

                biomeResourcesTemplate[key] = new BiomeResourcesTemplate
                {
                    Name = biomeID,
                    Resources = resourceID,
                    GatherRate = gatherRate
                };
            }
        }

        public static void ResetBiomeResourcesFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Default data JSON not found: {jsonFilePath}");

            string json = File.ReadAllText(jsonFilePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<BiomeResourcesTemplate>>>(json);

            if (jsonData != null && jsonData.ContainsKey("BiomeResource"))
            {
                DatabaseManager.WriteDB("DELETE FROM BiomeResource;", null);

                foreach (var br in jsonData["BiomeResource"])
                {
                    DatabaseManager.WriteDB(
                        "INSERT INTO BiomeResource (BiomeID, ResourceID, GatherRate) VALUES (@biome, @resource, @rate)",
                        new Dictionary<string, object>
                        {
                    { "@biome", br.Name},
                    { "@resource", br.Resources},
                    { "@rate", br.GatherRate }
                        });
                }

                biomeResourcesTemplate = null;
                LoadBiomeResourcesTemplate();
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
