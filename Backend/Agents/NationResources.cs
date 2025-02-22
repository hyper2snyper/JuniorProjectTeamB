using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    class NationResources
    {
        int currentGold;
        int currentWood;
        int currentIron;
        int currentStone;
        int currentFood;

        int woodDemand;
        int ironDemand;
        int stoneDemand;
        public int foodDemand;

        void SetWoodDemand()
        {
            if (currentWood <= 100 && woodDemand < 10)
                woodDemand++;
            else if (currentWood >= 500 && woodDemand > 0)
                woodDemand--;
        }
        void SetIronDemand()
        {
            if (currentIron <= 100 && ironDemand < 10)
                ironDemand++;
            else if (currentIron >= 500 && ironDemand > 0)
                ironDemand--;
        }
        void SetStoneDemand()
        {
            if (currentStone <= 100 && stoneDemand < 10)
                stoneDemand++;
            else if (currentStone >= 500 && stoneDemand > 0)
                stoneDemand--;
        }
        void SetFoodDemand()
        {
            if (currentFood <= 100 && foodDemand < 10)
                foodDemand++;
            else if (currentFood >= 500 && foodDemand > 0)
                foodDemand--;
        }
    }
}
