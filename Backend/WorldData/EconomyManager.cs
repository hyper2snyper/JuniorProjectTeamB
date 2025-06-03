using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Properties;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows.Documents;
using System.Xml.Linq;
using static JuniorProject.Backend.WorldData.EconomyManager;

namespace JuniorProject.Backend.WorldData
{
    public class EconomyManager : Serializable
    {
        public class Trade : Serializable
        {
            public Trade() { }
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
            public bool accepted; // distinguish between accepted/initiated trades for HistoryWindow

            public override void SerializeFields()
            {
                SerializeField<string>(initiator);
                SerializeField<string>(resource);
                SerializeField<int>(resourceAmount);
                SerializeField<int>(price);
                SerializeField<bool>(accepted);
            }

            public override void DeserializeFields()
            {
                initiator = DeserializeField<string>();
                resource = DeserializeField<string>();
                resourceAmount = DeserializeField<int>();
                price = DeserializeField<int>();
                accepted = DeserializeField<bool>();
            }
        }

        public class Demand : Serializable
        {
            public Demand() { }
            public Demand(string resource, int demand)
            {
                this.resource = resource;
                this.demand = demand;
            }

            public string resource;
            public float demand;

            public override void SerializeFields()
            {
                SerializeField<string>(resource);
                SerializeField<float>(demand);
            }

            public override void DeserializeFields()
            {
                resource = DeserializeField<string>();
                demand = DeserializeField<float>();
            }
        }

        public class Resource : Serializable
        {
            public Resource() { }
            public Resource(int price, int totalResource, int priceLevel, string name)
            {
                this.price = price;
                this.totalResource = totalResource;
                this.priceLevel = priceLevel;
                this.initialTotalResource = totalResource;
                this.name = name;
            }

            public string name;
            public int price;
            public int totalResource;
            public int priceLevel; // integer to keep track of updating the price (starts at 0 and goes up or down 1 depending on amount of resource)
            public int initialTotalResource;

            public override void SerializeFields()
            {
                SerializeField<string>(name);
                SerializeField<int>(price);
                SerializeField<int>(totalResource);
                SerializeField<int>(priceLevel);
                SerializeField<int>(initialTotalResource);
            }

            public override void DeserializeFields()
            {
                name = DeserializeField<string>();
                price = DeserializeField<int>();
                totalResource = DeserializeField<int>();
                priceLevel = DeserializeField<int>();
                initialTotalResource = DeserializeField<int>();
            }
        }

        public Dictionary<string, Nation> nations;
        public Dictionary<string, Demand> demands; // Key is nation-resource

        Dictionary<string, Resource> resources; // Key is resource name

        public List<Trade> potentialTrades;

        Dictionary<ulong, List<Resource>> itemsHistory; // Keep track of resources and their values every X ticks (putting it as 5 ticks initially)
        const ulong TICK_INTERVAL_FOR_ITEM_HISTORY = 5;
        Timer<ulong> historySave = new Timer<ulong>(0, TICK_INTERVAL_FOR_ITEM_HISTORY);
        List<Trade> tradesHistory; // Keep track of trades
        Dictionary<string, Dictionary<string, int>> nationResources = new Dictionary<string, Dictionary<string, int>>();

        static readonly string[] resourceTypes = { "Food", "Iron", "Wood", "Gold" };

        public ulong currentTick = 0;
        public ulong savedTick = 0;

        public EconomyManager()
        {

        }

        public void Initialize()
        {
            nations = ClientCommunicator.GetData<World>("World").nations;
            demands = new Dictionary<string, Demand>();
            resources = new Dictionary<string, Resource>();
            potentialTrades = new List<Trade>();
            itemsHistory = new Dictionary<ulong, List<Resource>>();
            tradesHistory = new List<Trade>();

            foreach (string type in resourceTypes)
            {
                using var results = DatabaseManager.ReadDB($"SELECT InitialPrice, InitialStartingAmount FROM Resources WHERE ResourceName='{type}'");
                int startingAmount = 0;
                while (results.Read())
                {
                    int initialPrice = results.GetInt32(0);
                    startingAmount = results.GetInt32(1);
                    resources[type] = new Resource(initialPrice, startingAmount * 3, 0, type); // multiply by 3, there's three teams
                }
                foreach (var n in nations.Values)
                {
                    demands[$"{n.color}-{type}"] = new Demand(type, startingAmount / (startingAmount * 3)); // Initializing demands
                    n.resources[type] = startingAmount;
                }
            }

            ClientCommunicator.RegisterData<Dictionary<ulong, List<Resource>>>("itemsHistory", itemsHistory);
            ClientCommunicator.RegisterData<List<Trade>>("tradesHistory", tradesHistory);
            ClientCommunicator.RegisterData<Dictionary<string, Dictionary<string, int>>>("nationResources", nationResources);
        }

        public void TakeTurn(ulong tickCount)
        {
            nations = ClientCommunicator.GetData<World>("World").nations;
            if (savedTick != 0)
            {
                tickCount += savedTick;
            }
            currentTick = tickCount;

            RespondToTrades(tickCount);
            UpdateResourceValues();
            CalculateDemands();
            if (tickCount > 1)
            {
                InitiatePotentialTrades();
            }
            //Print();
            if (historySave.Tick(tickCount))
            {
                ArchiveResourceInformation(tickCount);
            }
            ClientCommunicator.UpdateData<Dictionary<ulong, List<Resource>>>("itemsHistory", itemsHistory);
            ClientCommunicator.UpdateData<List<Trade>>("tradesHistory", tradesHistory);
            UpdateNationResourcesData();
        }

        /* --------- MAIN FUNCTIONS ---------------- */
        void ArchiveResourceInformation(ulong tickCount)
        {
            List<Resource> resourceList = new List<Resource>();
            foreach (var resource in resources)
            {
                resourceList.Add(new Resource(resource.Value.price, resource.Value.totalResource, resource.Value.priceLevel, resource.Value.name));
            }
            itemsHistory.Add(tickCount, resourceList);
        }
        void CalculateDemands()
        {
            foreach (var n in nations.Values)
            {
                foreach (string r in resourceTypes)
                {
                    demands[$"{n.color}-{r}"].demand = (1 - ((float)n.resources[r] / (float)resources[r].totalResource));
                }
            }
        }

        void InitiatePotentialTrades()
        {
            foreach (var nationDemand in demands)
            {
                string key = nationDemand.Key;
                string nation = key.Split("-")[0];
                string resouceType = key.Split("-")[1];

                using var results = DatabaseManager.ReadDB($"SELECT DemandPercentToinitiateTrade FROM Resources WHERE ResourceName='{demands[$"{nation}-{resouceType}"].resource}'");
                while (results.Read())
                {
                    double demandThresholdToInitiateTrade = results.GetDouble(0);
                    if (demands[$"{nation}-{resouceType}"].demand > demandThresholdToInitiateTrade)
                    {
                        int totalResourcesWanted = CalculateWantedResourceTotal(demands[$"{nation}-{resouceType}"].resource, nation);
                        potentialTrades.Add(new Trade(nation, demands[$"{nation}-{resouceType}"].resource, totalResourcesWanted, CalculateTradePrice(totalResourcesWanted, demands[$"{nation}-{resouceType}"].resource)));
                        Debug.Print($"ADDED POTENTIAL TRADE: {nation} : <- {demands[$"{nation}-{resouceType}"].resource} for {CalculateTradePrice(totalResourcesWanted, demands[$"{nation}-{resouceType}"].resource)} gold");
                    }
                }
            }
        }
        void RespondToTrades(ulong tickCount)
        {
            foreach (Trade t in potentialTrades)
            {
                List<string> possibleNations = new List<string>();
                foreach (var n in nations)
                {
                    string name = n.Key;
                    if (name == t.initiator) continue;
                    var nation = n.Value;

                    if (nation.resources[t.resource] > t.resourceAmount && ShouldAcceptTrade(tickCount, t.resource, demands[$"{nation.color}-{t.resource}"].demand))
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

        void UpdateResourceValues()
        {
            foreach (string r in resourceTypes)
            {
                resources[r].totalResource = CalculateResourceTotal(r);
                UpdatePrice(r);
            }
        }


        /* ------ HELPER FUNCTIONS --------- */
        void Print()
        {
            Debug.Print("\n----RESOURCES----");
            foreach (string r in resourceTypes)
            {
                Debug.Print($"Type: {r} | Price: {resources[r].price} | Total: {resources[r].totalResource}");
            }
        }

        void UpdateNationResourcesData()
        {
            var nationResourcesData = new Dictionary<string, Dictionary<string, int>>();
            foreach (var nation in nations.Values)
            {
                nationResourcesData[nation.color] = new Dictionary<string, int>(nation.resources);
            }

            ClientCommunicator.UpdateData<Dictionary<string, Dictionary<string, int>>>("nationResources", nationResourcesData);
        }

        bool ShouldAcceptTrade(ulong tickCount, string resource, double acceptingNationDemand)
        {
            using var results = DatabaseManager.ReadDB($"SELECT ChanceToAcceptTrade, DemandPercentToAcceptTrade FROM Resources WHERE ResourceName='{resource}'");

            while (results.Read())
            {
                double chanceToAccept = results.GetDouble(0);
                double demandThreshold = results.GetDouble(1);

                if (acceptingNationDemand > demandThreshold) return false;

                double value = Math.Abs(Math.Sin(tickCount * 12.9898) * 43758.5453) % 1.0; // Have not checked if this works yet
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
            using var results = DatabaseManager.ReadDB($"SELECT OffsetDemandPercentBy FROM Resources WHERE ResourceName='{resource}'");
            while (results.Read())
            {
                double offset = results.GetDouble(0);
                double newNationDemand = 1 - (demands[$"{nation}-{resource}"].demand - offset);
                return (int)(newNationDemand * resources[resource].totalResource);
            }
            Debug.Print("ERROR!!! Cannot read from database to decide to accept trade, returning false");
            return 0;
        }

        void UpdatePrice(string resource)
        {
            // dynamic pricing is buggy, idrk if we wanna implement it xdddd

            // TODO: Tweak with these settings a bit to get it working once resources are able to be generated and consumed
            //using var results = DatabaseManager.ReadDB($"SELECT ScalePercentAmount, InitialPrice FROM Resources WHERE ResourceName='{resource}'");
            //while (results.Read())
            //{
            //    double scalePercent = results.GetDouble(0);
            //    double threshold = resources[resource].totalResource * scalePercent;
            //    int delta = resources[resource].totalResource - resources[resource].initialTotalResource;
            //    int priceLevelChange = (int)(delta / threshold);

            //    int priceChange = priceLevelChange - resources[resource].priceLevel;
            //    if (priceChange != 0)
            //    {
            //        int initialPrice = results.GetInt32(1);
            //        int newPrice = (priceChange * 1) + resources[resource].price;
            //        resources[resource].price = Math.Max(initialPrice, newPrice);
            //        resources[resource].priceLevel = priceLevelChange;

            //    }
            //}
        }

        void AcceptTrade(Trade trade, Nation acceptingNation)
        {
            acceptingNation.resources[trade.resource] -= trade.resourceAmount;
            acceptingNation.resources["Gold"] += trade.price;

            nations[trade.initiator].resources[trade.resource] += trade.resourceAmount;
            nations[trade.initiator].resources["Gold"] -= trade.price;

            trade.accepted = true;
            tradesHistory.Add(trade);

            Debug.Print($"COMPLETED TRADE: {acceptingNation.color} : {trade.resourceAmount} {trade.resource} -> {trade.initiator} for {trade.price} gold");
        }

        public override void SerializeFields()
        {
            SerializeField<ulong, Resource>(itemsHistory);
            SerializeField<Trade>(tradesHistory);
            SerializeField<Trade>(potentialTrades);
            SerializeField<string, Resource>(resources);
            SerializeField<string, Demand>(demands);
            savedTick = currentTick;
            SerializeField<ulong>(savedTick);
        }

        public override void DeserializeFields()
        {
            itemsHistory = DeserializeNestedDictionary<ulong, Resource>();
            tradesHistory = DeserializeList<Trade>();
            potentialTrades = DeserializeList<Trade>();
            resources = DeserializeDictionary<string, Resource>();
            demands = DeserializeDictionary<string, Demand>();
            savedTick = DeserializeField<ulong>();

            ClientCommunicator.RegisterData<Dictionary<ulong, List<Resource>>>("itemsHistory", itemsHistory); // make this available to HistoryWindow
            ClientCommunicator.RegisterData<List<Trade>>("tradesHistory", tradesHistory); // make this available to HistoryWindow 
            ClientCommunicator.RegisterData<Dictionary<string, Dictionary<string, int>>>("nationResources", nationResources); // make this available to HistoryWindow
        }
    }
}
