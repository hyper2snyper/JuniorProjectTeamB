﻿using JuniorProject.Backend.WorldData;
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

namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : Window
    {

        Dictionary<string, Unit.UnitTemplate> unitTemplates;
        Dictionary<string, Building.BuildingTemplate> buildingTemplates; 
        Dictionary<(string Biome, string Resource), BiomeResourcesTemplate> biomeResourcesTemplate;


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
            SaveUnit("Cannon", CannonD, CannonR, CannonH);
            SaveUnit("Cavalier", CavalierD, CavalierR, CavalierH);
            SaveUnits();
            
            // Buildings
            SaveBuilding("Capital", CapitialCost);
            SaveBuilding("House", HouseCost);
            SaveBuilding("Mine", MineCost);
            SaveBuilding("Barracks", BarrackCost);
            SaveBuilding("Smith", SmithCost);
            SaveBuilding("Farm", FarmCost);

            //Biome Resources
            SaveBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
            SaveBiome("Highland", GoldH, WoodH, StoneH, FoodH, IronH);
            SaveBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
            SaveBiome("HighlandForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);

            BiomeResources.SaveBiomeResourcesTemplate();


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
                LoadUnit("Cannon", CannonD, CannonR, CannonH);
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
            }

            if (biomeResourcesTemplate == null)
            {
                BiomeResources.LoadBiomeResourcesTemplate();

                LoadBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
                LoadBiome("Highland", GoldH, WoodH, StoneH, FoodH, IronH);
                LoadBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
                LoadBiome("HighlandForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);
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

                if (buildingTemplates != null && buildingTemplates.TryGetValue(name, out var localTemplate))
                {
                    localTemplate.cost = cost;
                }

                if (buildingTemplates != null && buildingTemplates.TryGetValue(name, out var globalTemplate))
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
                if (BiomeResources.biomeResourcesTemplate.TryGetValue((biome, resource), out var global))
                    global.GatherRate = rate;

                if (biomeResourcesTemplate != null && biomeResourcesTemplate.TryGetValue((biome, resource), out var local))
                    local.GatherRate = rate;
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

        private void ResetClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalData", "DefaultVal.json");

                // Reset all three
                Unit.ResetUnitTemplatesFromJson(path);
                Building.ResetBuildingTemplatesFromJson(path);
                BiomeResources.ResetBiomeResourcesFromJson(path);

                // Reload UI: Units
                unitTemplates = Unit.unitTemplates;
                LoadUnit("Archer", ArcherD, ArcherR, ArcherH);
                LoadUnit("Soldier", SoldierD, SoldierR, SoldierH);
                LoadUnit("Catapult", CatapultD, CatapultR, CatapultH);
                LoadUnit("Cannon", CannonD, CannonR, CannonH);
                LoadUnit("Cavalier", CavalierD, CavalierR, CavalierH);

                // Reload UI: Buildings
                buildingTemplates = Building.buildingTemplates;
                LoadBuilding("Capital", CapitialCost);
                LoadBuilding("House", HouseCost);
                LoadBuilding("Mine", MineCost);
                LoadBuilding("Barracks", BarrackCost);
                LoadBuilding("Smith", SmithCost);
                LoadBuilding("Farm", FarmCost);

                // Reload UI: Biome Resources
                BiomeResources.LoadBiomeResourcesTemplate();
                LoadBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
                LoadBiome("Highland", GoldH, WoodH, StoneH, FoodH, IronH);
                LoadBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
                LoadBiome("HighlandForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);

                Debug.Print("All templates reset to default values.");
            }
            catch (Exception ex)
            {
                Debug.Print($"Error resetting templates: {ex.Message}");
            }
        }
    }
}
