using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    class Building : Serializable
    {

        class BuildingTemplate()
        {
            int maxHealth;

        }


        const int _fieldCount = -1;
        public override int fieldCount { get { return _fieldCount; } }


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
