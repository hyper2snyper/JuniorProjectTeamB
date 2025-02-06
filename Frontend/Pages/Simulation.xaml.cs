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
using JuniorProject.Frontend.Components;

namespace JuniorProject
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Page
    {
        Image mapImage;
        Drawer drawer;

        private string relativePath = "LocalData\\Map.png";
        public Simulation()
        {
            InitializeComponent();
            mapImage = this.Map;
            mapImage.Source = ReloadImage();
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
			ClientCommunicator.CallActionWaitFor("RegenerateWorld"); //First, tell the map to regenerate the world.
			mapImage.Source = ReloadImage();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {

        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            Debug.Print("Pause Clicked.");
            ClientCommunicator.CallAction("TogglePause");
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
            drawer = ClientCommunicator.GetData<Drawer>("Drawer");
            Drawing.Bitmap worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage"); //Get the data from the backend
            WriteableBitmap writeableBitmap = new WriteableBitmap(worldBitmap.Width, worldBitmap.Height, 96, 96, PixelFormats.Bgra32, null); //Create the writeableBitmap

            drawer.Draw(worldBitmap, ref writeableBitmap);

            return writeableBitmap;
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}