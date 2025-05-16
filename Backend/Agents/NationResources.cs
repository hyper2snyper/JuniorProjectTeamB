using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using JuniorProject.Properties;
using System.Resources;

namespace JuniorProject.Backend.Agents
{
    class NationResources
    {

        public Dictionary<string, int> ownedResources = new Dictionary<string, int>();
        public Dictionary<string, int> generationRates = new Dictionary<string, int>();
        public Dictionary<string, int> resourceDemands = new Dictionary<string, int>();

        public NationResources()
        {
            SQLiteDataReader results = DatabaseManager.ReadDB("SELECT * FROM Resources;");
            while (results.Read())
            {
                ownedResources[results.GetString(0)] = 0;
                generationRates[results.GetString(0)] = 0;
                resourceDemands[results.GetString(0)] = 5;
            }
        }

        public void TakeTurn()
        {
            foreach (var resource in ownedResources)
            {

                ownedResources[resource.Key] += generationRates[resource.Key];

                if (ownedResources[resource.Key] <= 100 && resourceDemands[resource.Key] < 10)
                    resourceDemands[resource.Key]++;
                else if (ownedResources[resource.Key] >= 500 && resourceDemands[resource.Key] > 0)
                    resourceDemands[resource.Key]--;

                Console.WriteLine(resource.Key + " " + resource.Value + "\n");
            }
            Console.WriteLine("---------------\n");
        }

    }
}
