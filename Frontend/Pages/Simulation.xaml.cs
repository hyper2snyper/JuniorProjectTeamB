using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace JuniorProject
{
    public partial class Simulation : Page
    {
        private DispatcherTimer simTimer;
        private TimeSpan simElapsedTime;

        public Simulation()
        {
            InitializeComponent();
            SimulationService.Instance.SetMapImage(Map);

            // Initialize Timer
            simTimer = new DispatcherTimer();
            simTimer.Interval = TimeSpan.FromSeconds(1); // Update every second
            simTimer.Tick += SimTimer_Tick;
            simElapsedTime = TimeSpan.Zero; // Start from 00:00:00
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            simElapsedTime = simElapsedTime.Add(TimeSpan.FromSeconds(1)); // Increment time
            TxtElapsedTime.Text = simElapsedTime.ToString(@"hh\:mm\:ss"); // Update label
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.StartSimulation();
            simTimer.Start(); // Start the timer
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.PauseSimulation();
            simTimer.Stop(); // Stop the timer

            MessageBox.Show("Simulation paused.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateSimulationDetails()
        {
            TxtSimulationTime.Text = simElapsedTime.ToString(@"hh\:mm\:ss");
            TxtElapsedTime.Text = simTimer.IsEnabled ? "Running" : "Stopped";
        }
        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.RefreshSimulation();
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
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.UpdateSimulationTime(TimeSpan.FromMinutes(5)); // Example update
            UpdateSimulationDetails();
        }
    }
}
