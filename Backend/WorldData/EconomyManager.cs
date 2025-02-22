using JuniorProject.Backend.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    class EconomyManager
    {
        int woodValue;
        int ironValue;
        int stoneValue;
        int foodValue;

        public EconomyManager()
        {
            woodValue = 100;
            ironValue = 100;
            stoneValue = 100;
            foodValue = 100;
        }

        void CalcWoodPrice(int woodDemand)
        {
            woodValue *= ((woodDemand - 5) / 20) + 1;
        }

        void CalcIronPrice(int ironDemand)
        {
            ironValue *= ((ironDemand - 5) / 20) + 1;
        }

        void CalcStonePrice(int stoneDemand)
        {
            stoneValue *= ((stoneDemand - 5) / 20) + 1;
        }

        void CalcFoodPrice(NationResources buyer)
        {
            foodValue *= ((buyer.foodDemand - 5) / 20) + 1;
        }
    }
}
