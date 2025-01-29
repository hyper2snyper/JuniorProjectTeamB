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
using Drawing = System.Drawing;

namespace JuniorProject
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Page
    {
        Image mapImage;

        private string relativePath = "LocalData\\Map.png";
        public Simulation()
        {
            InitializeComponent();

            mapImage = this.Map;

            ClientCommunicator.CallAction<IState>("SetState", new Backend.States.Simulation());
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            mapImage.Source = ReloadImage();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {

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

        private async void SaveClicked(object sender, RoutedEventArgs e)
        {
            //We can save the .png in a background thread
            await Task.Run(() =>
            {
                ClientCommunicator.CallAction("SaveWorld");
            });
        }



        private WriteableBitmap ReloadImage()
        {
            ClientCommunicator.CallActionWaitFor("RegenerateWorld"); //First, tell the map to regenerate the world.
            Drawing.Bitmap worldBitmap;
            worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage"); //Get the data from the backend
            WriteableBitmap writeableBitmap = new WriteableBitmap(worldBitmap.Width, worldBitmap.Height, 96, 96, PixelFormats.Bgra32, null); //Create the writeableBitmap
            byte[] pixels = new byte[4 * (worldBitmap.Width * worldBitmap.Height)]; //pixel color buffer. each color is four bytes.
            for (int y = 0; y < worldBitmap.Width; y++)
            {
                for (int x = 0; x < worldBitmap.Height; x++)
                {
                    Drawing.Color c = worldBitmap.GetPixel(x, y);
                    int pos = ((y * worldBitmap.Width) + x) * 4;
                    pixels[pos] = c.B;
                    pixels[pos + 1] = c.G;
                    pixels[pos + 2] = c.R;
                    pixels[pos + 3] = c.A;
                }
            }
            writeableBitmap.WritePixels(new Int32Rect(0, 0, worldBitmap.Width, worldBitmap.Height), pixels, (worldBitmap.Width * 4), 0); //Update the bitmap
            return writeableBitmap;
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}