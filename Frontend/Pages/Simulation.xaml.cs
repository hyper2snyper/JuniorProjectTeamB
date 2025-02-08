using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Navigation;
using System.IO;
using JuniorProject.Backend;
namespace JuniorProject
{
    public partial class Simulation : Page
    {
        private DispatcherTimer simTimer;
        private TimeSpan simElapsedTime;
        public TimeSpan SimulationTime { get; private set; }

        WriteableBitmap map;
        Image mapImage;
        public string relativePath = "LocalData\\Map.png";

        public Simulation()
        {
            InitializeComponent();

            map = new WriteableBitmap(100, 100, 96, 96, PixelFormats.Bgr32, null);
            mapImage = this.Map;
            mapImage.Source = map;
            SimulationTime = TimeSpan.Zero;
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
            simTimer.Start(); // Start the timer
            Console.WriteLine("Simulation started.");
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            simTimer.Stop(); // Stop the timer
            Console.WriteLine("Simulation paused.");
        }

        private void UpdateSimulationDetails()
        {
            TxtSimulationTime.Text = simElapsedTime.ToString(@"hh\:mm\:ss");
            TxtElapsedTime.Text = simTimer.IsEnabled ? "Running" : "Stopped";
            SimulationTime = simElapsedTime;
            Console.WriteLine($"Simulation time updated: {SimulationTime}");
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Simulation refreshed.");
        }
        private async void SaveClicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                ClientCommunicator.CallAction("SaveWorld");
            });
            Console.WriteLine("Simulation state saved.");
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateSimulationDetails();
        }
    }
}
