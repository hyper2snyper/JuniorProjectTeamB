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
            public Demand(string resource, int demand)
            { 
                this.resource = resource;
                this.demand = demand;
            }

            public string resource;
            public int demand;
        }

        struct Resource
        {
            public Resource(int price, int totalResource, int priceLevel)
            {
                this.price = price;
                this.totalResource = totalResource;
                this.priceLevel = priceLevel;
            }

            public int price;
            public int totalResource;
            public int priceLevel; // integer to keep track of updating the price (starts at 0 and goes up or down 1 depending on amount of resource)
        }

        public Dictionary<string, Nation> nations;
        Dictionary<string, Demand> demands; // Key is nation name

        Dictionary<string, Resource> resources; // Key is resource name

        List<Trade> potentialTrades;

        static readonly string[] resourceTypes = { "Food", "Iron", "Wood", "Gold" };

        public EconomyManager() { }

        public void Initialize(ref Dictionary<string, Nation> nations)
        {
            this.nations = nations;
            demands = new Dictionary<string, Demand>();
            resources = new Dictionary<string, Resource>();
            potentialTrades = new List<Trade>();

            foreach (string type in resourceTypes)
            {
                using var results = DatabaseManager.ReadDB($"SELECT InitialPrice, InitialStartingAmount FROM Resources WHERE ResourceName='{type}'");
                while (results.Read()) {
                    int initialPrice = results.GetInt32(0);
                    int startingAmount = results.GetInt32(1) * 3; // multiply by 3, there's three teams
                    resources[type] = new Resource(initialPrice, startingAmount, 0); // Read initial price & have a set initial value of total resource from database
                }

                foreach (var n in nations.Values)
                {
                    demands[n.color] = new Demand(type, 0); // Initializing demands
                    n.resources[type] = 50; // arbritray number, pull from database later
                }
            }
        }

        public void TakeTurn(ulong tickCount)
        {
            RespondToTrades(tickCount);
            CalculateDemands();
            InitiatePotentialTrades();
        }

        /* --------- MAIN FUNCTIONS ---------------- */
        void CalculateDemands()
        {
            foreach (var n in nations.Values)
            {
                Demand d = demands[n.color];
                Dictionary<string, int> resourceDemands = new Dictionary<string, int>();
                foreach (string r in resourceTypes)
                {
                    resourceDemands[r] = 1 - (n.resources[r] / resources[r].totalResource);
                }
                d.resource = resourceDemands.MaxBy(entry => entry.Value).Key;
                d.demand = resourceDemands.Values.Max();
                demands[n.color] = d;
            }
        }

        void InitiatePotentialTrades()
        {
            foreach (var nationDemand in demands)
            {
                string nation = nationDemand.Key;
                Demand demand = nationDemand.Value;

                using var results = DatabaseManager.ReadDB($"SELECT DemandPercentToinitiateTrade FROM Resources WHERE ResourceName='{demand.resource}'");
                while (results.Read())
                {
                    double demandThresholdToInitiateTrade = results.GetDouble(0);
                    if (demand.demand > demandThresholdToInitiateTrade)
                    {
                        int totalResourcesWanted = CalculateWantedResourceTotal(demand.resource, nation);
                        potentialTrades.Add(new Trade(nation, demand.resource, totalResourcesWanted, CalculateTradePrice(totalResourcesWanted, demand.resource)));
                    }
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

                    if (nation.resources[t.resource] > t.resourceAmount && ShouldAcceptTrade(tickCount, t.resource, demands[name].demand))
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
        bool ShouldAcceptTrade(ulong tickCount, string resource, double acceptingNationDemand)
        {
            // TODO: Implement relations between nations perhaps? This might be a bit tricker to implement, but we can start off with implementing an algorithim 
            // that uses the tick rate to accept trade or not
            using var results = DatabaseManager.ReadDB($"SELECT ChanceToAcceptTrade, DemandPercentToAcceptTrade FROM Resources WHERE ResourceName='{resource}'");

            while (results.Read()) {
                double chanceToAccept = results.GetDouble(0);
                double demandThreshold = results.GetDouble(1);

                if (acceptingNationDemand > demandThreshold) return false;

                double value = Math.Abs(Math.Sin(tickCount * 12.9898) * 43758.5453) % 1.0;
                return value < chanceToAccept;
            }

            Debug.Print("ERROR!!! Cannot read from database to decide to accept trade, returning false");
            return false;
        }

        int CalculateResourceTotal(string resource)
        {
            int total = 0;
            foreach (var n in nations)
            {
                total += n.Value.resources[resource];
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
            using var results = DatabaseManager.ReadDB($"SELECT OffsetDemandPercentBy FROM Resources WHERE ResourceName='{resource}'");
            while (results.Read())
            {
                double offset = results.GetDouble(0);
                double newNationDemand = 1 - (demands[nation].demand - offset);
                return (int)(newNationDemand * resources[resource].totalResource);
            }
            Debug.Print("ERROR!!! Cannot read from database to decide to accept trade, returning false");
            return 0;
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

            Debug.Print($"COMPLETED TRADE: {acceptingNation.color} : {trade.resourceAmount} {trade.resource} -> {trade.initiator} for {trade.price} gold");
        }
    }
}
