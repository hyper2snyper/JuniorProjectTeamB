


using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.Agents
{
    class Unit : Serializable
    {
        class UnitTemplate //The template of the unit, the type if you will.
        {
            public string name;
            public string description;
            public int attackDamage;
            public int attackRange;
            public int maxHealth;
            public int sprite;
            public int flags; //PLACEHOLDER

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
                    sprite = results.GetInt32(5);
                    flags = results.GetInt32(6); //PLACEHOLDER
                }
            }
        }

        UnitTemplate unitType;
        public int health;

        IObjective? objective;

        Map.Tile pos;

        public override int fieldCount
        {
            get { return 2; }
        }

        public Unit()
        {
            
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


        public override void SerializeFields()
        {

        }

        public override void DeserializeFields()
        {

        }
    }
}
