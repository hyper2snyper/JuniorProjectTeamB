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
            ReloadImage();
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
            ReloadImage();
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

        public void DrawPixel(int row, int column)
        {
            try
            {
                // Reserve the back buffer for updates.
                map.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = map.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * map.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Compute the pixel's color.
                    int color_data = 255 << 16; // R
                    color_data |= 128 << 8;   // G
                    color_data |= 255 << 0;   // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
                }

                // Specify the area of the bitmap that changed.
                map.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                map.Unlock();
            }
        }
        private void ReloadImage()
        {
                // Combine the current directory with the relative path
                string imagePath = Path.Combine(Environment.CurrentDirectory, relativePath);

                if (File.Exists(imagePath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        mapImage.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Image file not found: {imagePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
}
