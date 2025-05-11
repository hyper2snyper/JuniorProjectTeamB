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

        public Dictionary<string, int> ownedResources = new Dictionary<string, int>();
        public Dictionary<string, int> resourceDemands = new Dictionary<string, int>();

        public NationResources()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Resources;");
            while (results.Read())
            {
                ownedResources[results.GetString(0)] = 0;
                resourceDemands[results.GetString(0)] = 5;
            }
        }

        void SetOwnedResource(string resourceType, int changeValue)
        {
            ownedResources[resourceType] += changeValue;
        }
        void SetResourceDemand(string resourceType)
        {
            if (ownedResources[resourceType] <= 100 && resourceDemands[resourceType] < 10)
                resourceDemands[resourceType]++;
            else if (ownedResources[resourceType] >= 500 && resourceDemands[resourceType] > 0)
                resourceDemands[resourceType]--;
        }

        public void TakeTurn(string resourceType, int changeValue)
        {
            SetOwnedResource(resourceType, changeValue);
            SetResourceDemand(resourceType);
        }

    }
}
