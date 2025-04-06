using JuniorProject.Backend.WorldData;
using JuniorProject.Backend;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : Window
    {
        int ADamage,SDamage,CavDamage,CatDamage,CanDamage;
        int ARange,SRange,CavRange,CatRange,CanRange;
        int AHealth,SHealth,CavHealth,CatHealth,CanHealth;
        float CapBuildingCost, HouseBuildingCost, MineBuildingCost, BarracksBuildingCost, SmithBuildingCost, FarmBuildingCost;
        int GrassGoldGatherRate, GrassWoodGatherRate, GrassStoneGatherRate, GrassFoodGatherRate, GrassIronGatherRate,
            HighGoldGatherRate, HighWoodGatherRate, HighStoneGatherRate, HighFoodGatherRate, HighIronGatherRate,
            ForestGoldGatherRate, ForestWoodGatherRate, ForestStoneGatherRate, ForestFoodGatherRate, ForestIronGatherRate,
            HFGoldGatherRate, HFWoodGatherRate, HFStoneGatherRate, HFFoodGatherRate, HFIronGatherRate;

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
            Debug.Print("Saved!!!!!!");
            this.Hide();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        //Archer
        private void ArcherDamage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ArcherD.Text == "") return;
            ADamage = int.Parse(ArcherD.Text);
        }
        private void ArcherRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ArcherR.Text == "") return;
            ARange = int.Parse(ArcherR.Text);
        }
        private void ArcherHealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ArcherH.Text == "") return;
            AHealth = int.Parse(ArcherH.Text);
        }
        //Catapult
        private void CatapultDamage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CatapultD.Text == "") return;
            CatDamage = int.Parse(CatapultD.Text);
        }
        private void CatapultRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CatapultR.Text == "") return;
            CatRange = int.Parse(CatapultR.Text);
        }
        private void CatapultHealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CatapultH.Text == "") return;
            CatHealth = int.Parse(CatapultH.Text);
        }
        //Soldier
        private void SoldierDamage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SoldierD.Text == "") return;
            SDamage = int.Parse(SoldierD.Text);
        }
        private void SoldierRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SoldierR.Text == "") return;
            SRange = int.Parse(SoldierR.Text);
        }
        private void SoldierHealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SoldierH.Text == "") return;
            SHealth = int.Parse(SoldierH.Text);
        }
        //Cannon
        private void CannonDamage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CannonD.Text == "") return;
            CanDamage = int.Parse(CannonD.Text);
        }
        private void CannonRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CannonR.Text == "") return;
            CanRange = int.Parse(CannonR.Text);
        }
        private void CannonHealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CannonH.Text == "") return;
            CanHealth = int.Parse(CannonH.Text);
        }
        //Cavalier
        private void CavalierDamage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CavalierD.Text == "") return;
             CavDamage = int.Parse(CavalierD.Text);
        }
        private void CavalierRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CavalierR.Text == "") return;
            CavRange = int.Parse(CavalierR.Text);
        }
        private void CavalierHealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CavalierH.Text == "") return;
            CavHealth = int.Parse(CavalierH.Text);
        }
        //Grasslands
        private void GrasslandWood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WoodG.Text == "") return;
            GrassWoodGatherRate = int.Parse(WoodG.Text);
        }
        private void GrasslandIron_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IronG.Text == "") return;
            GrassIronGatherRate = int.Parse(IronG.Text);
        }
        private void GrasslandStone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StoneG.Text == "") return;
            GrassStoneGatherRate = int.Parse(StoneG.Text);
        }
        private void GrasslandFood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoodG.Text == "") return;
            GrassFoodGatherRate = int.Parse(FoodG.Text);
        }
        private void GrasslandGold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GoldG.Text == "") return;
            GrassGoldGatherRate = int.Parse(GoldG.Text);
        }
        //Highlands
        private void HighlandWood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WoodH.Text == "") return;
            HighWoodGatherRate = int.Parse(WoodH.Text);
        }
        private void HighlandIron_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IronH.Text == "") return;
            HighIronGatherRate = int.Parse(IronH.Text);
        }
        private void HighlandStone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StoneH.Text == "") return;
            HighStoneGatherRate = int.Parse(StoneH.Text);
        }
        private void HighlandFood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoodH.Text == "") return;
            HighFoodGatherRate = int.Parse(FoodH.Text);
        }
        private void HighlandGold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GoldH.Text == "") return;
            HighGoldGatherRate = int.Parse(GoldH.Text);
        }
        //Forest
        private void ForestWood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WoodF.Text == "") return;
            ForestWoodGatherRate = int.Parse(WoodF.Text);
        }
        private void ForestIron_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IronF.Text == "") return;
            ForestIronGatherRate = int.Parse(IronF.Text);
        }
        private void ForestStone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StoneF.Text == "") return;
            ForestStoneGatherRate = int.Parse(StoneF.Text);
        }
        private void ForestFood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoodF.Text == "") return;
            ForestFoodGatherRate = int.Parse(FoodF.Text);
        }
        private void ForestGold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GoldF.Text == "") return;
            ForestGoldGatherRate = int.Parse(GoldF.Text);
        }
        //Highlands Forest
        private void HighlandFWood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WoodHF.Text == "") return;
            HFWoodGatherRate = int.Parse(WoodHF.Text);
        }
        private void HighlandFIron_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IronHF.Text == "") return;
            HFIronGatherRate = int.Parse(IronHF.Text);
        }
        private void HighlandFStone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StoneHF.Text == "") return;
            HFStoneGatherRate = int.Parse(StoneHF.Text);
        }
        private void HighlandFFood_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FoodHF.Text == "") return;
            HFFoodGatherRate = int.Parse(FoodHF.Text);
        }
        private void HighlandFGold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GoldHF.Text == "") return;
            HFGoldGatherRate = int.Parse(GoldHF.Text);
        }
        //Buildings
        private void CapitalBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CapitialCost.Text == "" || CapitialCost.Text == ".") return;
            CapBuildingCost = float.Parse(CapitialCost.Text);
        }
        private void HouseBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (HouseCost.Text == "" || HouseCost.Text == ".") return;
            HouseBuildingCost = float.Parse(HouseCost.Text);
        }
        private void MineBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MineCost.Text == "" || MineCost.Text == ".") return;
            MineBuildingCost = float.Parse(MineCost.Text);
        }
        private void BarracksBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BarrackCost.Text == "" || BarrackCost.Text == ".") return;
            BarracksBuildingCost = float.Parse(BarrackCost.Text);
        }
        private void SmithBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SmithCost.Text == "" || SmithCost.Text == ".") return;
            SmithBuildingCost = float.Parse(SmithCost.Text);
        }
        private void FarmBC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FarmCost.Text == "" || FarmCost.Text == ".") return;
            FarmBuildingCost = float.Parse(FarmCost.Text);
        }

    }
}
