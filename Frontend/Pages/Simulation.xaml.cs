using System.Data.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using JuniorProject.Backend;

namespace JuniorProject
{
	/// <summary>
	/// Interaction logic for Simulation.xaml
	/// </summary>
	public partial class Simulation : Page
	{
		WriteableBitmap map;
		Image mapImage;

        private string relativePath = "LocalData\\Map.png";
		public Simulation() { 
			InitializeComponent();

			map = new WriteableBitmap(100, 100, 96, 96, PixelFormats.Bgr32, null);
			mapImage = this.Map;
			mapImage.Source = map;

            
		}

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            ClientCommunicator.CallActionWaitFor("RegenerateWorld");
            LoadImage();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Pause";
            string caption = "Information";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
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

        private void LoadImage()
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

        public void BackToMainMenu(object sender, RoutedEventArgs e) 
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}