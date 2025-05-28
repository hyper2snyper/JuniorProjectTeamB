using JuniorProject.Backend.WorldData;
using JuniorProject.Backend;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.IO;
using JuniorProject.Backend.Agents;
using JuniorProject.Frontend.Windows;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using static JuniorProject.Backend.Agents.BiomeResources;
using System.Text.Json.Serialization;
using System.Data.SQLite;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : Window
    {
        class ResourceTemplate
        {
            [JsonPropertyName("ResourceName")]
            public string ResourceName { get; set; }

            [JsonPropertyName("InitialStartingAmount")]
            public int? InitialStartingAmount { get; set; }

            [JsonPropertyName("InitialPrice")]
            public int? InitialPrice { get; set; }

            [JsonPropertyName("ScalePercentAmount")]
            public double? ScalePercentAmount { get; set; }

            [JsonPropertyName("DemandPercentToInitiateTrade")]
            public double? DemandPercentToInitiateTrade { get; set; }

            [JsonPropertyName("DemandPercentToAcceptTrade")]
            public double? DemandPercentToAcceptTrade { get; set; }

            [JsonPropertyName("ChanceToAcceptTrade")]
            public double? ChanceToAcceptTrade { get; set; }

            [JsonPropertyName("OffsetDemandPercentBy")]
            public double? OffsetDemandPercentBy { get; set; }

            public static Dictionary<string, ResourceTemplate> resourcesTemplate = new();

            public static void LoadResourcesTemplate()
            {
                if (resourcesTemplate == null)
                    resourcesTemplate = new Dictionary<string, ResourceTemplate>();
                else if (resourcesTemplate.Count > 0)
                    return;

                resourcesTemplate.Clear();

                using var results = DatabaseManager.ReadDB("SELECT * FROM Resources");

                while (results.Read())
                {
                    var resource = results.GetString(0);
                    var initialStartingAmount = results.GetInt32(1);
                    var initialPrice = results.GetInt32(2);
                    var scalePercentAmount = results.GetDouble(3);
                    var demandPercentToInitiateTrade = results.GetDouble(4);
                    var demandPercentToAcceptTrade = results.GetDouble(5);
                    var chanceToAcceptTrade = results.GetDouble(6);
                    var offsetDemandPercentBy = results.GetDouble(7);

                    resourcesTemplate[resource] = new ResourceTemplate
                    {
                        ResourceName = resource,
                        InitialStartingAmount = initialStartingAmount,
                        InitialPrice = initialPrice,
                        ScalePercentAmount = scalePercentAmount,
                        DemandPercentToInitiateTrade = demandPercentToInitiateTrade,
                        DemandPercentToAcceptTrade = demandPercentToAcceptTrade,
                        OffsetDemandPercentBy = offsetDemandPercentBy,
                        ChanceToAcceptTrade= chanceToAcceptTrade,
                    };
                }
            }

            public static void ResetResourcesFromJson(string jsonFilePath)
            {
                if (!File.Exists(jsonFilePath))
                    throw new FileNotFoundException($"Default data JSON not found: {jsonFilePath}");

                string json = File.ReadAllText(jsonFilePath);
                var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<ResourceTemplate>>>(json);

                if (jsonData != null && jsonData.ContainsKey("Resources"))
                {
                    foreach (var res in jsonData["Resources"])
                    {
                        if (string.IsNullOrWhiteSpace(res.ResourceName) || string.IsNullOrWhiteSpace(res.ResourceName)) continue;

                        DatabaseManager.WriteDB(
                        @"UPDATE Resources SET 
                            InitialStartingAmount = @initialStartingAmount,
                            InitialPrice = @initialPrice,
                            ScalePercentAmount = @scalePercentAmount,
                            DemandPercentToInitiateTrade = @demandInitiate,
                            DemandPercentToAcceptTrade = @demandAccept,
                            ChanceToAcceptTrade = @chanceAccept,
                            OffsetDemandPercentBy = @offsetDemand
                          WHERE ResourceName = @resourceName",
                        new Dictionary<string, object>
                        {
                            { "@initialStartingAmount", res.InitialStartingAmount },
                            { "@initialPrice", res.InitialPrice },
                            { "@scalePercentAmount", res.ScalePercentAmount },
                            { "@demandInitiate", res.DemandPercentToInitiateTrade },
                            { "@demandAccept", res.DemandPercentToAcceptTrade },
                            { "@chanceAccept", res.ChanceToAcceptTrade },
                            { "@offsetDemand", res.OffsetDemandPercentBy },
                            { "@resourceName", res.ResourceName }
                        });
                    }

                    resourcesTemplate = null;
                    LoadResourcesTemplate();
                }
            }

            public static void SaveResourcesTemplate()
            { 
                foreach (var template in resourcesTemplate.Values)
                {
                    DatabaseManager.WriteDB(
                    @"UPDATE Resources SET 
                        InitialStartingAmount = @InitialStartingAmount,
                        InitialPrice = @InitialPrice,
                        ScalePercentAmount = @ScalePercentAmount,
                        DemandPercentToInitiateTrade = @DemandPercentToInitiateTrade,
                        DemandPercentToAcceptTrade = @DemandPercentToAcceptTrade,
                        ChanceToAcceptTrade = @ChanceToAcceptTrade,
                        OffsetDemandPercentBy = @OffsetDemandPercentBy
                      WHERE ResourceName = @ResourceName",
                    new Dictionary<string, object>
                    {
                        { "@InitialStartingAmount", template.InitialStartingAmount },
                        { "@InitialPrice", template.InitialPrice },
                        { "@ScalePercentAmount", template.ScalePercentAmount },
                        { "@DemandPercentToInitiateTrade", template.DemandPercentToInitiateTrade },
                        { "@DemandPercentToAcceptTrade", template.DemandPercentToAcceptTrade },
                        { "@ChanceToAcceptTrade", template.ChanceToAcceptTrade },
                        { "@OffsetDemandPercentBy", template.OffsetDemandPercentBy },
                        { "@ResourceName", template.ResourceName }
                    }
                    );
                }
            }
        }

        Dictionary<string, Unit.UnitTemplate> unitTemplates;
        Dictionary<string, Building.BuildingTemplate> buildingTemplates;
        Dictionary<(string Biome, string Resource), BiomeResourcesTemplate> biomeResourcesTemplate;
        Dictionary<string, ResourceTemplate> resourceTemplate;

        public static readonly Regex intOnly = new Regex("[0-9]+");
        public static readonly Regex correctFloat = new Regex("^[0-9]*\\.?[0-9]*$");
        public static readonly Regex correctFloatNeg = new Regex("^-?[0-9]*\\.?[0-9]*$");

        public SimulationSettings()
        {
            InitializeComponent();
        }
        //Preview Text Input
        private void NumberOnlyInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !intOnly.IsMatch(e.Text);
        }
        private void FloatOnlyInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !correctFloat.IsMatch(e.Text);
        }
        private void SavedClicked(object sender, RoutedEventArgs e)
        {

            // Units
            SaveUnit("Archer", ArcherD, ArcherR, ArcherH);
            SaveUnit("Soldier", SoldierD, SoldierR, SoldierH);
            SaveUnit("Catapult", CatapultD, CatapultR, CatapultH);
            SaveUnit("Cannoneer", CannonD, CannonR, CannonH);
            SaveUnit("Cavalier", CavalierD, CavalierR, CavalierH);
            SaveUnits();


            // Buildings
            // Buildings
            SaveBuilding("Capital", CapitialCost);
            SaveBuilding("House", HouseCost);
            SaveBuilding("Mine", MineCost);
            SaveBuilding("Barracks", BarrackCost);
            SaveBuilding("Smith", SmithCost);
            SaveBuilding("Farm", FarmCost);
            SaveBuilding("Port", PortCost);

            Building.SaveAllBuildingTemplates();

            //Biome Resources
            SaveBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
            SaveBiome("Highlands", GoldH, WoodH, StoneH, FoodH, IronH);
            SaveBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
            SaveBiome("HighlandsForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);


            //Resources for economy
            SaveResource("Wood", WoodResourceStart, WoodResourceStartPrice, WoodResourceScalePercAmount, WoodResourceDemandPercInit, WoodResourceDemandPercAcc, WoodResourceChanceAccept, WoodResourceOffsetDemand);
            SaveResource("Gold", GoldResourceStart, null, null, null, null, null, null);
            SaveResource("Iron", IronResourceStart, IronResourceStartPrice, IronResourceScalePercAmount, IronResourceDemandPercInit, IronResourceDemandPercAcc, IronResourceChanceAccept, IronResourceOffsetDemand);
            SaveResource("Food", FoodResourceStart, FoodResourceStartPrice, FoodResourceScalePercAmount, FoodResourceDemandPercInit, FoodResourceDemandPercAcc, FoodResourceChanceAccept, FoodResourceOffsetDemand);


            Debug.Print("Saved!");
            this.Hide();
        }


        private void SaveUnits()
        {
            Unit.SaveAllUnitTemplates();
        }


        private void CancelClicked(object sender, RoutedEventArgs e)
        {

            this.Hide();

            unitTemplates = null;
            buildingTemplates = null;
            biomeResourcesTemplate = null;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (unitTemplates == null)
            {
                Unit.LoadUnitTemplates();
                unitTemplates = new Dictionary<string, Unit.UnitTemplate>(Unit.unitTemplates);

                LoadUnit("Archer", ArcherD, ArcherR, ArcherH);
                LoadUnit("Soldier", SoldierD, SoldierR, SoldierH);
                LoadUnit("Catapult", CatapultD, CatapultR, CatapultH);
                LoadUnit("Cannoneer", CannonD, CannonR, CannonH);
                LoadUnit("Cavalier", CavalierD, CavalierR, CavalierH);
            }

            if (buildingTemplates == null)
            {
                Building.LoadBuildingTemplates();
                buildingTemplates = new Dictionary<string, Building.BuildingTemplate>(Building.buildingTemplates);

                LoadBuilding("Capital", CapitialCost);
                LoadBuilding("House", HouseCost);
                LoadBuilding("Mine", MineCost);
                LoadBuilding("Barracks", BarrackCost);
                LoadBuilding("Smith", SmithCost);
                LoadBuilding("Farm", FarmCost);
                LoadBuilding("Port", PortCost);
            }

            if (biomeResourcesTemplate == null)
            {
                BiomeResources.LoadBiomeResourcesTemplate();

                LoadBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
                LoadBiome("Highlands", GoldH, WoodH, StoneH, FoodH, IronH);
                LoadBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
                LoadBiome("HighlandsForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);
            }

            if (resourceTemplate == null)
            { 
                ResourceTemplate.LoadResourcesTemplate();
                LoadResource("Gold", GoldResourceStart, null, null, null, null, null, null);
                LoadResource("Food", FoodResourceStart, FoodResourceStartPrice, FoodResourceScalePercAmount, FoodResourceDemandPercInit, FoodResourceDemandPercAcc, FoodResourceChanceAccept, FoodResourceOffsetDemand);
                LoadResource("Wood", WoodResourceStart, WoodResourceStartPrice, WoodResourceScalePercAmount, WoodResourceDemandPercInit, WoodResourceDemandPercAcc, WoodResourceChanceAccept, WoodResourceOffsetDemand);
                LoadResource("Iron", IronResourceStart, IronResourceStartPrice, IronResourceScalePercAmount, IronResourceDemandPercInit, IronResourceDemandPercAcc, IronResourceChanceAccept, IronResourceOffsetDemand);
            }
        }



        private void LoadUnit(string name, TextBox dmg, TextBox rng, TextBox hp)
        {
            if (!unitTemplates.ContainsKey(name)) return;
            var u = unitTemplates[name];
            dmg.Text = u.attackDamage.ToString();
            rng.Text = u.attackRange.ToString();
            hp.Text = u.maxHealth.ToString();
        }
        private void SaveUnit(string name, TextBox dmg, TextBox rng, TextBox hp)
        {
            if (int.TryParse(dmg.Text, out int damage) &&
                int.TryParse(rng.Text, out int range) &&
                int.TryParse(hp.Text, out int health))
            {
                if (unitTemplates.TryGetValue(name, out var localTemplate))
                {
                    localTemplate.attackDamage = damage;
                    localTemplate.attackRange = range;
                    localTemplate.maxHealth = health;
                }

                if (Unit.unitTemplates.TryGetValue(name, out var globalTemplate))
                {
                    globalTemplate.attackDamage = damage;
                    globalTemplate.attackRange = range;
                    globalTemplate.maxHealth = health;
                }
            }
        }

        private void LoadBuilding(string name, TextBox field)
        {
            if (buildingTemplates.TryGetValue(name, out var template))
                field.Text = template.cost.ToString();
        }

        private void SaveBuilding(string name, TextBox costField)
        {
            if (int.TryParse(costField.Text, out int cost))
            {
                if (buildingTemplates.TryGetValue(name, out var localTemplate))
                {
                    localTemplate.cost = cost;
                }

                if (Building.buildingTemplates.TryGetValue(name, out var globalTemplate))
                {
                    globalTemplate.cost = cost;
                }
            }
        }

        private void SaveBiome(string biomeName, TextBox gold, TextBox wood, TextBox stone, TextBox food, TextBox iron)
        {
            SaveBiomeResource(biomeName, "Gold", gold);
            SaveBiomeResource(biomeName, "Wood", wood);
            SaveBiomeResource(biomeName, "Stone", stone);
            SaveBiomeResource(biomeName, "Food", food);
            SaveBiomeResource(biomeName, "Iron", iron);
        }

        private void SaveBiomeResource(string biome, string resource, TextBox field)
        {
            if (int.TryParse(field.Text, out int rate))
            {
                var key = (biome, resource);
                if (BiomeResources.biomeResourcesTemplate.TryGetValue(key, out var template))
                {
                    template.GatherRate = rate;
                }

                // Now persist the updated value
                BiomeResources.SaveBiomeResourcesTemplate();
            }
        }

        private void SaveResource(string resourceName, TextBox startAmount, TextBox startPrice, TextBox priceScalePercent, TextBox demandInitTradePercent, TextBox demandAcceptTradePercent, TextBox acceptPercent, TextBox offsetDemandPercentBy)
        {
            if (ResourceTemplate.resourcesTemplate.TryGetValue(resourceName, out var resourceTemplate))
            {
                if (int.TryParse(startAmount.Text, out int amount))
                {
                    resourceTemplate.InitialStartingAmount = amount;
                }

                if (resourceName == "Gold") {
                    ResourceTemplate.SaveResourcesTemplate();
                    return;
                } // Don't set other values for Gold

                if (int.TryParse(startPrice.Text, out int price))
                {
                    resourceTemplate.InitialPrice = price;
                }
                if (double.TryParse(priceScalePercent.Text, out double priceScale))
                {
                    resourceTemplate.ScalePercentAmount = priceScale;
                }
                if (double.TryParse(demandInitTradePercent.Text, out double demandInit))
                {
                    resourceTemplate.DemandPercentToInitiateTrade = demandInit;
                }
                if (double.TryParse(demandAcceptTradePercent.Text, out double demandAccept))
                {
                    resourceTemplate.DemandPercentToAcceptTrade = demandAccept;
                }
                if (double.TryParse(acceptPercent.Text, out double acceptPerc))
                {
                    resourceTemplate.ChanceToAcceptTrade = acceptPerc;
                }
                if (double.TryParse(offsetDemandPercentBy.Text, out double offsetPercent))
                {
                    resourceTemplate.OffsetDemandPercentBy = offsetPercent;
                }
                ResourceTemplate.SaveResourcesTemplate();
            }
        }

        private void LoadBiome(string biomeName, TextBox gold, TextBox wood, TextBox stone, TextBox food, TextBox iron)
        {
            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Gold"), out var goldT))
                gold.Text = goldT.GatherRate.ToString();

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Wood"), out var woodT))
                wood.Text = woodT.GatherRate.ToString();

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Stone"), out var stoneT))
                stone.Text = stoneT.GatherRate.ToString();

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Food"), out var foodT))
                food.Text = foodT.GatherRate.ToString();

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Iron"), out var ironT))
                iron.Text = ironT.GatherRate.ToString();
        }

        private void LoadResource(string resourceName, TextBox startAmount, TextBox startPrice, TextBox priceScalePercent, TextBox demandInitTradePercent, TextBox demandAcceptTradePercent, TextBox acceptPercent, TextBox offsetDemandPercentBy)
        {
            if (ResourceTemplate.resourcesTemplate.TryGetValue(resourceName, out var resource))
            { 
                startAmount.Text = resource.InitialStartingAmount.ToString();
                
                if (resourceName == "Gold") return; // Don't set other values for Gold

                startPrice.Text = resource.InitialPrice.ToString();
                priceScalePercent.Text = resource.ScalePercentAmount.ToString();
                demandInitTradePercent.Text = resource.DemandPercentToInitiateTrade.ToString();
                demandAcceptTradePercent.Text = resource.DemandPercentToAcceptTrade.ToString();
                acceptPercent.Text = resource.ChanceToAcceptTrade.ToString();
                offsetDemandPercentBy.Text = resource.OffsetDemandPercentBy.ToString();
            }
        }

        private void ResetClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = $"{Properties.Resources.ProjectDir}\\LocalData\\DefaultVal.json";

                // Reset all three (DB gets wiped + repopulated)
                Unit.ResetUnitTemplatesFromJson(path);
                Building.ResetBuildingTemplatesFromJson(path);
                BiomeResources.ResetBiomeResourcesFromJson(path);
                ResourceTemplate.ResetResourcesFromJson(path);

                // Reload from DB to in-memory
                Unit.LoadUnitTemplates();
                Building.LoadBuildingTemplates();
                BiomeResources.LoadBiomeResourcesTemplate();
                ResourceTemplate.LoadResourcesTemplate();

                // Refresh local cache
                unitTemplates = new Dictionary<string, Unit.UnitTemplate>(Unit.unitTemplates);
                buildingTemplates = new Dictionary<string, Building.BuildingTemplate>(Building.buildingTemplates);
                biomeResourcesTemplate = new Dictionary<(string, string), BiomeResourcesTemplate>(BiomeResources.biomeResourcesTemplate);
                resourceTemplate = new Dictionary<string, ResourceTemplate>(ResourceTemplate.resourcesTemplate);

                // Update UI
                LoadUnit("Archer", ArcherD, ArcherR, ArcherH);
                LoadUnit("Soldier", SoldierD, SoldierR, SoldierH);
                LoadUnit("Catapult", CatapultD, CatapultR, CatapultH);
                LoadUnit("Cannoneer", CannonD, CannonR, CannonH);
                LoadUnit("Cavalier", CavalierD, CavalierR, CavalierH);

                LoadBuilding("Capital", CapitialCost);
                LoadBuilding("House", HouseCost);
                LoadBuilding("Mine", MineCost);
                LoadBuilding("Barracks", BarrackCost);
                LoadBuilding("Smith", SmithCost);
                LoadBuilding("Farm", FarmCost);
                LoadBuilding("Port", PortCost);

                LoadBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
                LoadBiome("Highlands", GoldH, WoodH, StoneH, FoodH, IronH);
                LoadBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
                LoadBiome("HighlandsForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);

                LoadResource("Gold", GoldResourceStart, null, null, null, null, null, null);
                LoadResource("Food", FoodResourceStart, FoodResourceStartPrice, FoodResourceScalePercAmount, FoodResourceDemandPercInit, FoodResourceDemandPercAcc, FoodResourceChanceAccept, FoodResourceOffsetDemand);
                LoadResource("Wood", WoodResourceStart, WoodResourceStartPrice, WoodResourceScalePercAmount, WoodResourceDemandPercInit, WoodResourceDemandPercAcc, WoodResourceChanceAccept, WoodResourceOffsetDemand);
                LoadResource("Iron", IronResourceStart, IronResourceStartPrice, IronResourceScalePercAmount, IronResourceDemandPercInit, IronResourceDemandPercAcc, IronResourceChanceAccept, IronResourceOffsetDemand);

                Debug.Print("All templates reset to default values.");
            }
            catch (Exception ex)
            {
                Debug.Print($"Error resetting templates: {ex.Message}");
            }
        }

    }
}
