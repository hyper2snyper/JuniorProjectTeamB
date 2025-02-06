


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
            public int maxHealth;
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
