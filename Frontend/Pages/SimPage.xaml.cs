using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for SimPage.xaml
    /// </summary>
    public partial class SimPage : Page
    {
        private DateTime simStartTime;
        private bool isRunning;
        public SimPage()
        {
            InitializeComponent();
            simStartTime = DateTime.Now;
            isRunning = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            
            TxtSimulationTime.Text = DateTime.Now.ToString("HH:mm:ss");
            TxtElapsedTime.Text = (DateTime.Now - simStartTime).ToString(@"hh\:mm\:ss");
        }
    }
}
