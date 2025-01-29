


using System.IO;

namespace JuniorProject.Backend.WorldData
{
    class Unit : Serializable
    {
        struct UnitTemplate //The template of the unit, the type if you will.
        {
            public string name;
            public string description;
            public int maxHealth;
        }

        UnitTemplate unitType;
        public int health;


        Map.Tile pos;

        public override int fieldCount
        {
            get { return 2; }
        }

        public Unit()
        {

        }

        public override void SerializeFields(List<byte[]> serializedFields)
        {
            serializedFields.Add(SerializeField(health));
            serializedFields.Add(SerializeField("12345"));
        }

        public override void Deserialize(BinaryReader reader)
        {
            health = reader.ReadInt32();
            string r = reader.ReadString();
        }
    }
}
