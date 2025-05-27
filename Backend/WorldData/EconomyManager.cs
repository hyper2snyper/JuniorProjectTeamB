using JuniorProject.Backend.Agents;
using JuniorProject.Properties;

namespace JuniorProject.Backend.WorldData
{
    public class EconomyManager
    {
        struct Trade
        {
            public Trade(string i, string r, int rA, int p)
            {
                initiator = i;
                resource = r;
                resourceAmount = rA;
                price = p;
            }

            public string initiator;
            public string resource;
            public int resourceAmount;
            public int price;
        }

        struct Demand
        {
            public string resource;
            public int demand;
        }

        struct Resource
        {
            public int price;
            public int totalResource;
            public int priceLevel; // integer to keep track of updating the price (starts at 0 and goes up or down 1 depending on amount of resource)
        }

        public Dictionary<string, Nation> nations;
        Dictionary<string, Demand> demands; // Key is nation name

        Dictionary<string, Resource> resources; // Key is resource name

        List<Trade> potentialTrades;

        static readonly string[] resourceTypes = { "Food", "Iron", "Wood" };

        public EconomyManager() { }

        public void Initialize(ref Dictionary<string, Nation> nations)
        {
            this.nations = nations;
            demands = new Dictionary<string, Demand>();
            resources = new Dictionary<string, Resource>();

            foreach (string r in resourceTypes)
            {
                /* TODO: Read flat, initial price from database and intiialize the resources and demands dictionaries */
            }
        }

        public void TakeTurn(ulong tickCount)
        {
            /* uncomment when ready
             * 
            
            RespondToTrades(tickCount);
            CalculateDemands();
            InitiatePotentialTrades();

             */
        }

        /* --------- MAIN FUNCTIONS ---------------- */
        void CalculateDemands()
        {
            foreach (var n in nations.Values)
            {
                Demand d = demands[n.name];
                Dictionary<string, int> resourceDemands = new Dictionary<string, int>();
                foreach (string r in resourceTypes)
                {
                    resourceDemands[r] = 1 - (n.ownedResources[r] / resources[r].totalResource);
                }
                d.resource = resourceDemands.MaxBy(entry => entry.Value).Key;
                d.demand = resourceDemands.Values.Max();
                demands[n.name] = d;
            }
        }

        void InitiatePotentialTrades()
        {
            const double demandThresholdToInitiateTrade = 0.6; // TODO: arbritrary number currently; implement and receive from database

            foreach (var nationDemand in demands)
            {
                string nation = nationDemand.Key;
                Demand demand = nationDemand.Value;

                if (demand.demand > demandThresholdToInitiateTrade)
                {
                    int totalResourcesWanted = CalculateWantedResourceTotal(demand.resource, nation);
                    potentialTrades.Add(new Trade(nation, demand.resource, totalResourcesWanted, CalculateTradePrice(totalResourcesWanted, demand.resource)));
                }
            }
        }

        void RespondToTrades(ulong tickCount)
        {
            foreach (Trade t in potentialTrades)
            {
                // TODO: Implement, ensure initiator is not the trader
                // Find nation with demand that meets criteria to accept trade.
                List<string> possibleNations = new List<string>();

                foreach (var n in nations)
                {
                    string name = n.Key;
                    if (name == t.initiator) continue;

                    var nation = n.Value;

                    if (nation.resources[t.resource] > t.resourceAmount)
                    {
                        possibleNations.Add(name);
                    }
                }

                if (possibleNations.Count == 0) continue;
                if (possibleNations.Count == 1)
                {
                    AcceptTrade(t, nations[possibleNations[0]]);
                }

                // Scramble index out of possibleNations to have some variety even though it's deterministic, otherwise I'm afraid it will only go to one nation or something 
                int acceptingNationIndex = (int)tickCount % possibleNations.Count;
                AcceptTrade(t, nations[possibleNations[acceptingNationIndex]]);
            }
            potentialTrades.Clear();
        }


        /* ------ HELPER FUNCTIONS --------- */
        int CalculateResourceTotal(string resource)
        {
            int total = 0;
            foreach (var n in nations)
            {
                total += n.Value.ownedResources[resource];
            }
            return total;
        }

        int CalculateTradePrice(int totalResources, string resource)
        {
            return totalResources * resources[resource].price;
        }

        int CalculateWantedResourceTotal(string resource, string nation)
        {
            /* 
               Offset nation's demand by a certain %
            */

            const double offsetResourcePercentage = 0.05; // TODO: arbritrary number currently; implement and receive from database

            double newNationDemand = 1 - (demands[nation].demand - offsetResourcePercentage);

            return (int)(newNationDemand * resources[resource].totalResource);
        }

        void CheckToUpdatePrice(string resource)
        {
            /* 
             TODO: Retrieve current price of resource and update it with +/- percentage from database.

            Calculate if the price needs to be updated using the percent resource scale from database and the the current price point level (which starts at 1)
            I think it's possible, but I'm not entirely sure.

            If the calculated pricepoint level is not the same as the current one, update the price point depending if it's below or above.
            Update price with flat update price from database
             */
        }

        void AcceptTrade(Trade trade, Nation acceptingNation)
        {
            acceptingNation.resources[trade.resource] -= trade.resourceAmount;
            acceptingNation.resources["Gold"] += trade.price;

            nations[trade.initiator].resources[trade.resource] += trade.resourceAmount;
            nations[trade.initiator].resources["Gold"] -= trade.price;
        }
    }
}
