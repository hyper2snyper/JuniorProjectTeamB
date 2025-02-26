using JuniorProject.Backend;
using JuniorProject.Backend.States;
using JuniorProject.Frontend.Pages;
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
using System.Windows.Shapes;

namespace JuniorProject
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void StartSimulation(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MapCreation());
        }

        private void Catalog(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Other Simulations";
            string caption = "Coming Soon";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ApplicationSettings());
        }

        private void LoadSimulation(object sender, RoutedEventArgs e)
        {
            var fileSelect = new Microsoft.Win32.OpenFileDialog();
            fileSelect.DefaultExt = ".chs";
            fileSelect.Filter = "Cry Havoc Save (.chs)|*.chs";
            fileSelect.DefaultExt = "\\LocalData\\";
            if (fileSelect.ShowDialog() != true) return;

            ClientCommunicator.CallAction<IState>("SetState", new Backend.States.Loading(
                (Backend.States.Simulation simulation, ref bool loadingDone) =>
                {
                    simulation.LoadFromFile(fileSelect.FileName);
                    loadingDone = true;
                    ClientCommunicator.UpdateData<bool>("LoadingDone", loadingDone);
                }));
            NavigationService.Navigate(new Frontend.Pages.Loading());

        }
    }
}