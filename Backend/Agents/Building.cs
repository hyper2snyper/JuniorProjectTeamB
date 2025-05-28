using JuniorProject.Backend.WorldData;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JuniorProject.Frontend.Components;
using System.Drawing.Printing;

namespace JuniorProject.Backend.Agents
{
    public class Building : Mob
    {

        public class BuildingTemplate
        {
            [JsonPropertyName("BuildingName")]
            public string name { get; set; }

            [JsonPropertyName("BuildingCost")]
            public int cost { get; set; }

            [JsonPropertyName("Health")]
            public int maxHealth { get; set; }

            [JsonPropertyName("Sprite")] // Prevent changing sprite from JSON
            public string sprite { get; set; }

            [JsonPropertyName("HasColor")] // Prevent changing color flag from JSON
            public bool hasColor { get; set; }

            [JsonPropertyName("Capital")] // Prevent changing color flag from JSON
            public int capital { get; set; }
        }

        public static Dictionary<string, BuildingTemplate> buildingTemplates;
        public static BuildingTemplate capitalTemplate;
        public static void LoadBuildingTemplates()
        {
            if (buildingTemplates != null && buildingTemplates.Count > 0)
                return;

            buildingTemplates = new Dictionary<string, BuildingTemplate>();
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
            while (results.Read())
            {
                BuildingTemplate template = new BuildingTemplate
                {
                    name = results.GetString(0),
                    cost = results.GetInt32(1),
                    maxHealth = results.GetInt32(2),
                    sprite = results.GetString(3),
                    hasColor = results.GetBoolean(4),
                };

                if (results.GetInt32(5) != 0)
                {
                    capitalTemplate = template;
                }

                buildingTemplates.Add(template.name, template);
            }
        }

        public static void SaveAllBuildingTemplates()
        {
            if (buildingTemplates == null) return;

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

        public BuildingTemplate template;

        int health;

        public Building() { }
        public Building(string type, TileMap map, TileMap.Tile tile, Nation? nation) : base(map, tile, nation)
        {
            if (!buildingTemplates.ContainsKey(type))
            {
                Debug.Print($"Building was initialized with nonexistant type. {type}");
                return;
            }
            SetType(buildingTemplates[type]);
            drawableType = GenericDrawable.DrawableType.Building;
        }

        public static void ResetBuildingTemplatesFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Default data JSON not found: {jsonFilePath}");

            string json = File.ReadAllText(jsonFilePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<BuildingTemplate>>>(json);

            if (jsonData != null && jsonData.ContainsKey("Buildings"))
            {
                foreach (var t in jsonData["Buildings"])
                {
                    if (string.IsNullOrWhiteSpace(t.name)) continue;

                    DatabaseManager.WriteDB(
                        "INSERT OR REPLACE INTO Buildings " +
                        "(BuildingName, BuildingCost, Health, Sprite, HasColor, Capital) " +
                        "VALUES (@name, @cost, @health, @sprite, @hasColor, @capital)",
                        new Dictionary<string, object>
                        {
                            { "@name", t.name },
                            { "@cost", t.cost },
                            { "@health", t.maxHealth },
                            { "@sprite", t.sprite ?? "" },
                            { "@hasColor", t.hasColor ? 1 : 0 },
                            { "@capital", t.name == "Capital" ? 1 : 0 }
                        });
                }

                buildingTemplates = null;
                LoadBuildingTemplates();
            }
        }


        void SetType(BuildingTemplate template)
        {
            this.template = template;
            health = template.maxHealth;
            sprite = template.sprite;
        }

        public override void TakeTurn(ulong tick)
        {
            base.TakeTurn(tick);

            if (nation == null || this.pos == null)
                return;

            // Find the largest terrain percentage of the tile and set as the biome
            string biome = this.pos.primaryBiome;
            string resource;

            switch (template.name)
            {
                case "House":
                    break;
                case "Capital":
                    string[] capitalResources = { "Food", "Iron", "Wood", "Gold" };
                    foreach (var r in capitalResources)
                    {
                        double gatherRate = GetGatherRate(biome, r);
                        nation.resources[r] += (int)Math.Round(gatherRate);
                    }
                    break;
                case "Barracks":
                    break;
                case "Mine":
                    string[] mineResources = { "Iron", "Gold" };
                    foreach (var r in mineResources)
                    {
                        double gatherRate = GetGatherRate(biome, r);
                        nation.resources[r] += (int)Math.Round(gatherRate);
                    }
                    break;
                case "Smith":
                    break;
                case "Farm":
                    string[] farmResources = { "Food", "Wood" };
                    foreach (var r in farmResources)
                    {
                        double gatherRate = GetGatherRate(biome, r);
                        nation.resources[r] += (int)Math.Round(gatherRate);
                    }
                    break;
                case "Port":
                    break;
                default:
                    Debug.Print($"ERROR!!! Incorrect template name for nation {template.name}");
                    break;
            }
        }

        private double GetGatherRate(string biome, string resource)
        {
            using (var reader = DatabaseManager.ReadDB(
                $"SELECT GatherRate FROM BiomeResource WHERE BiomeID = '{biome}' AND ResourceID = '{resource}'"))
            {
                if (reader.Read())
                    return Convert.ToDouble(reader["GatherRate"]);
                Debug.Print($"ERROR!!! Cannot find gather rate for {biome} and {resource}");
                return 0.0;
            }
        }

        public override string GetSprite()
        {
            return $"{(template.hasColor ? (nation?.color) : "")}{base.GetSprite()}";
        }

        public override void SerializeFields()
        {
            base.SerializeFields();
            SerializeField(template.name);

            SerializeField(health);
            SerializeField(sprite);

        }

        public override void DeserializeFields()
        {
            base.DeserializeFields();
            template = buildingTemplates[DeserializeField<string>()];

            health = DeserializeField<int>();
            sprite = DeserializeField<string>();
        }




    }
}