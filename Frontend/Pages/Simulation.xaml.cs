using System.Windows;
using System.Windows.Controls;

namespace JuniorProject
{
    public partial class Simulation : Page
    {
        public Simulation()
        {
            InitializeComponent();

            // Pass the UI Image to SimulationService so it can update the map
            SimulationService.Instance.SetMapImage(Map);
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.RefreshSimulation();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.StartSimulation();
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.PauseSimulation();
            MessageBox.Show("Simulation paused.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SaveClicked(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                SimulationService.Instance.SaveSimulation();
            });

            MessageBox.Show("Simulation saved.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void SimPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SimPage());
        }
    }
}
