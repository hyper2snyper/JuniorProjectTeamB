using JuniorProject.Backend.Agents;

namespace JuniorProject.Backend.WorldData
{
    class EconomyManager
    {
        struct Trade
        {
            string initiator;
            string resource;
            int resourceAmount;
            int price;
        }

        struct Demand
        {
            string resource;
            int demand;
        }

        struct Resource
        {
            int price;
            int totalResource;
            int priceLevel;
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

        public void TakeTurn()
        {
            /* uncomment when ready
            
            RespondToTrades();
            CalculateDemands();
            InitiatePotentialTrades();

             */
        }

        /* --------- MAIN FUNCTIONS ---------------- */
        void CalculateDemands()
        {
            foreach (Nation n in nations.Values)
            {
                /* 
                  TODO: Demand = 1 - (Nation Resource / Total Resource in World)
                */
            }
        }

        void InitiatePotentialTrades()
        {
            foreach (string nation in demands.Keys)
            {
                /* 
                 TODO: Check if nation's demand is over the criteria demand set in the database
                If so, append trade to potentialTrades
                 */
            }
        }

        void RespondToTrades()
        {
            foreach (Trade t in potentialTrades)
            {
                // TODO: Implement, ensure initiator is not the trader
                // Find nation with demand that meets criteria to accept trade.
            }
        }


        /* ------ HELPER FUNCTIONS --------- */
        int CalculateResourceTotal(string resource)
        {
            // Update totalResources dictionary
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
    }
}
