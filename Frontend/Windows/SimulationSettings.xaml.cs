using JuniorProject.Backend.WorldData;
using JuniorProject.Backend;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using JuniorProject.Backend.Agents;
namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : Window
    {
        Dictionary<string, Unit.UnitTemplate> unitTemplates;
        Dictionary<string, Building.BuildingTemplate> buildingTemplates;

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
            //Units
            SaveUnit("Archer", ArcherD, ArcherR, ArcherH);
            SaveUnit("Soldier", SoldierD, SoldierR, SoldierH);
            SaveUnit("Catapult", CatapultD, CatapultR, CatapultH);
            SaveUnit("Cannon", CannonD, CannonR, CannonH);
            SaveUnit("Cavalier", CavalierD, CavalierR, CavalierH);
            SaveUnitsToDatabase();

            //Buildings
            SaveBuilding("Capital", CapitialCost);
            SaveBuilding("House", HouseCost);
            SaveBuilding("Barracks", BarrackCost);
            SaveBuilding("Mine", MineCost);
            SaveBuilding("Smith", SmithCost);
            SaveBuilding("Farm", FarmCost);
            Building.SaveBuildingTemplates();

            //BiomeResources
            SaveBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
            SaveBiome("Highland", GoldH, WoodH, StoneH, FoodH, IronH);
            SaveBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
            SaveBiome("HighlandForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);

            BiomeResources.SaveBiomeResourcesTemplate();

            Debug.Print("Saved!");
            this.Hide();
        }

        private void SaveUnit(string name, TextBox dmg, TextBox rng, TextBox hp)
        {
            if (!unitTemplates.ContainsKey(name)) return;
            var u = unitTemplates[name];
            u.attackDamage = int.Parse(dmg.Text);
            u.attackRange = int.Parse(rng.Text);
            u.maxHealth = int.Parse(hp.Text);
        }

        private void SaveUnitsToDatabase()
        {
            Unit.SaveAllUnitTemplates();
        }


        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Unit.LoadUnitTemplates(); // Loads from DB
            unitTemplates = new Dictionary<string, Unit.UnitTemplate>(Unit.unitTemplates);
            //Units
            LoadUnit("Archer", ArcherD, ArcherR, ArcherH);
            LoadUnit("Soldier", SoldierD, SoldierR, SoldierH);
            LoadUnit("Catapult", CatapultD, CatapultR, CatapultH);
            LoadUnit("Cannon", CannonD, CannonR, CannonH);
            LoadUnit("Cavalier", CavalierD, CavalierR, CavalierH);

            //Buildings
            Building.LoadBuildingTemplates();
            buildingTemplates = Building.buildingTemplates;

            LoadBuilding("Capital", CapitialCost);
            LoadBuilding("House", HouseCost);
            LoadBuilding("Barracks", BarrackCost);
            LoadBuilding("Mine", MineCost);
            LoadBuilding("Smith", SmithCost);
            LoadBuilding("Farm", FarmCost);

            //Biomes
            BiomeResources.LoadBiomeResourcesTemplate();
            LoadBiome("Grassland", GoldG, WoodG, StoneG, FoodG, IronG);
            LoadBiome("Highland", GoldH, WoodH, StoneH, FoodH, IronH);
            LoadBiome("Forest", GoldF, WoodF, StoneF, FoodF, IronF);
            LoadBiome("HighlandForest", GoldHF, WoodHF, StoneHF, FoodHF, IronHF);


        }

        private void SaveBiome(string biomeName, TextBox gold, TextBox wood, TextBox stone, TextBox food, TextBox iron)
        {
            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Gold"), out var goldT))
                goldT.GatherRate = int.Parse(gold.Text);

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Wood"), out var woodT))
                woodT.GatherRate = int.Parse(wood.Text);

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Stone"), out var stoneT))
                stoneT.GatherRate = int.Parse(stone.Text);

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Food"), out var foodT))
                foodT.GatherRate = int.Parse(food.Text);

            if (BiomeResources.biomeResourcesTemplate.TryGetValue((biomeName, "Iron"), out var ironT))
                ironT.GatherRate = int.Parse(iron.Text);
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


        private void LoadUnit(string name, TextBox dmg, TextBox rng, TextBox hp)
        {
            if (!unitTemplates.ContainsKey(name)) return;
            var u = unitTemplates[name];
            dmg.Text = u.attackDamage.ToString();
            rng.Text = u.attackRange.ToString();
            hp.Text = u.maxHealth.ToString();
        }

        private void LoadBuilding(string name, TextBox field)
        {
            if (buildingTemplates.TryGetValue(name, out var template))
                field.Text = template.cost.ToString();
        }

        private void SaveBuilding(string name, TextBox field)
        {
            if (buildingTemplates.TryGetValue(name, out var template) && int.TryParse(field.Text, out var val))
                template.cost = val;
        }


    }
}
