using JuniorProject.Backend;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JuniorProject.Frontend.Windows;
namespace JuniorProject.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for MapCreation.xaml
    /// </summary>
    public partial class MapCreation : Page
    {
        string seed;
        float amp;
        float freq;
        int octaves;
        float seaLevel;
        float treeLine;

        public static readonly Regex intOnly = new Regex("[0-9]+");
        public static readonly Regex correctFloat = new Regex("^[0-9]*\\.?[0-9]*$");
        public static readonly Regex correctFloatNeg = new Regex("^-?[0-9]*\\.?[0-9]*$");

        public MapCreation()
        {
            InitializeComponent();
        }

        private void Seed_TextChanged(object sender, TextChangedEventArgs e)
        {
            seed = Seed.Text;
        }

        private void Amp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !correctFloat.IsMatch(Amp.Text + e.Text);
        }

        private void Freq_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !correctFloat.IsMatch(Freq.Text + e.Text);
        }

        private void Octaves_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !intOnly.IsMatch(e.Text);
        }

        private void SeaLevel_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !correctFloatNeg.IsMatch(SeaLevel.Text + e.Text);
        }

        private void TreeLine_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !correctFloatNeg.IsMatch(TreeLine.Text + e.Text);
        }

        private void Amp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Amp.Text == "" || Amp.Text == ".") return;
            amp = float.Parse(Amp.Text);
        }

        private void Freq_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Freq.Text == "" || Freq.Text == ".") return;
            freq = float.Parse(Freq.Text);
        }

        private void Octaves_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Octaves.Text == "") return;
            octaves = int.Parse(Octaves.Text);
        }

        private void SeaLevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SeaLevel.Text == "" || SeaLevel.Text == "." || SeaLevel.Text == "-") return;
            seaLevel = float.Parse(SeaLevel.Text);
            seaLevel = float.Clamp(seaLevel, -1, 1);
        }

        private void TreeLine_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeLine.Text == "" || TreeLine.Text == "." || TreeLine.Text == "-") return;
            treeLine = float.Parse(TreeLine.Text);
            treeLine = float.Clamp(treeLine, -1, 1);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            ClientCommunicator.CallAction<IState>("SetState", new Backend.States.Loading(seed, amp, freq, octaves, seaLevel, treeLine));
            NavigationService.Navigate(new Loading());
        }

<<<<<<< HEAD
        private void SimSettings_Click(object sender, RoutedEventArgs e)
        {
            SimulationSettings simSettings = new SimulationSettings();
            simSettings.Show();
            Console.WriteLine("SimSettings Windows");
        }
=======
>>>>>>> main
    }
}