using System.Data.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Controls = System.Windows.Controls;
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
using System.Drawing;

namespace JuniorProject
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Page
    {
        Controls.Image mapImage;
        Canvas mapCanvas;
        Drawer drawer;
        int imageCounter = 0;

        private string relativePath = "LocalData\\Map.png";
        public Simulation()
        {
            InitializeComponent();
            mapCanvas = this.Canvas;
            drawer = new Drawer(ref mapCanvas);
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
			ClientCommunicator.CallActionWaitFor("RegenerateWorld"); //First, tell the map to regenerate the world.
            drawer.Initialize();
            drawer.Draw();
        }

        public void SetGridlines(object sender, RoutedEventArgs e)
        { 
            drawer.SetGridlines();
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

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}