using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    class NationResources
    {

        Dictionary<string, int> ownedResources = new Dictionary<string, int>();
        Dictionary<string, int> resourceDemands = new Dictionary<string, int>();

        public NationResources()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Resources;");
            while (results.Read())
            {
                ownedResources[results.GetString(1)] = 0;
                resourceDemands[results.GetString(1)] = 5;
            }
        }

        void SetResourceDemand(string resourceType)
        {
            if (ownedResources[resourceType] <= 100 && resourceDemands[resourceType] < 10)
                resourceDemands[resourceType]++;
            else if (ownedResources[resourceType] >= 500 && resourceDemands[resourceType] > 0)
                resourceDemands[resourceType]--;
        }

    }
}
