using System.IO;
using JuniorProject.Backend.Helpers;
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

        public class Test : Serializable
        {
            public string name;
            public int test;
            public float test2;
            public override void DeserializeFields()
            {
                name = DeserializeField<string>();
                test = DeserializeField<int>();
                test2 = DeserializeField<float>();
            }

            public override void SerializeFields()
            {
                SerializeField(name);
                SerializeField(test);
                SerializeField(test2);
            }
        }
        public Test t1;

        IObjective? objective;

        TileMap.Tile pos;
        Vector2Int posV;

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