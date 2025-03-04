using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System;
using System.Data.SQLite;


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
            buildingTemplates = new Dictionary<string, BuildingTemplate>();
			SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
			while (results.Read())
			{
                BuildingTemplate template = new BuildingTemplate();
				template.name = results.GetString(0);
				template.cost = results.GetInt32(1);
                template.maxHealth = results.GetInt32(2);
				template.sprite = results.GetString(3);
                template.hasColor = results.GetBoolean(4);
				if (results.GetInt32(5) != 0)
                {
                    capitalTemplate = template;
                }
                buildingTemplates.Add(template.name, template);
			}
		}

        BuildingTemplate template;

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