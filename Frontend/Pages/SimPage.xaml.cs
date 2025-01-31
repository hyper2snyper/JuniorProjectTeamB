using System;
using System.Windows;
using System.Windows.Controls;

namespace JuniorProject
{
    public partial class SimPage : Page
    {
        public SimPage()
        {
            InitializeComponent();
            UpdateSimulationDetails();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.StartSimulation();
            UpdateSimulationDetails();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.UpdateSimulationTime(TimeSpan.FromMinutes(5)); // Example update
            UpdateSimulationDetails();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Simulation());
        }

        private void UpdateSimulationDetails()
        {
            TxtSimulationTime.Text = SimulationService.Instance.SimulationTime.ToString(@"hh\:mm\:ss");
            TxtElapsedTime.Text = SimulationService.Instance.IsRunning ? "Running" : "Stopped";
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Settings";
            string caption = "Coming Soon";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SimulationService.Instance.RefreshSimulation();
            UpdateSimulationDetails();
        }
    }
}
