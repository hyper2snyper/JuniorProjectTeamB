﻿using JuniorProject.Backend.WorldData;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;


namespace JuniorProject.Backend.Agents
{
    public class Building : Mob
    {

        public class BuildingTemplate
        {
            public string name;
            public int cost;
            public int maxHealth;
            public string sprite;
            public bool hasColor = false;
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
                    hasColor = results.GetBoolean(4)
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
            if(!buildingTemplates.ContainsKey(type))
            {
                Debug.Print($"Building was initialized with nonexistant type. {type}");
                return;
            }
            SetType(buildingTemplates[type]);
        }

        public static void ResetBuildingTemplatesFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Default data JSON not found: {jsonFilePath}");

            string json = File.ReadAllText(jsonFilePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<BuildingTemplate>>>(json);

            if (jsonData != null && jsonData.ContainsKey("Buildings"))
            {
                DatabaseManager.WriteDB("DELETE FROM Buildings;", null);

                foreach (var template in jsonData["Buildings"])
                {
                    DatabaseManager.WriteDB(
                        "INSERT INTO Buildings (BuildingName, BuildingCost, BuildingMaxHealth, Sprite, HasColor, IsCapital) " +
                        "VALUES (@name, @cost, @maxHealth, @sprite, @hasColor, @isCapital)",
                        new Dictionary<string, object>
                        {
                    { "@name", template.name },
                    { "@cost", template.cost },
                    { "@maxHealth", template.maxHealth },
                    { "@sprite", template.sprite },
                    { "@hasColor", template.hasColor },
                    { "@isCapital", template.name == "Capital" ? 1 : 0 }
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
            if(template.name == "Farm" && nation != null)
            {
                nation.money++;
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