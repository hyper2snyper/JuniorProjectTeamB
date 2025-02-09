using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    class Building : Serializable
    {

		class BuildingTemplate
		{
			string name;
			int cost;
			int maxHealth;
			int sprite;

			public BuildingTemplate()
			{
				SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Buildings;");
				while (results.Read())
				{
					name = results.GetString(0);
					cost = results.GetInt32(2);
					sprite = results.GetInt32(3);
				}
			}
		}

		Nation owner;
		int health;



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
