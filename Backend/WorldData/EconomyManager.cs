using JuniorProject.Backend.Agents;
using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    class EconomyManager
    {

        Dictionary<string, int> resourceValues = new Dictionary<string, int>();

        public EconomyManager()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Resources;");
            while (results.Read())
            {
                resourceValues[results.GetString(1)] = 100;
            }
        }

        void CalcResourcePrice(string resourceType, int resourceDemand)
        {
            resourceValues[resourceType] *= ((resourceDemand - 5) / 20) + 1;
        }
    }
}
