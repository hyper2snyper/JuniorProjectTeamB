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
using System.Windows.Shapes;

namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : Window
    {
        public SimulationSettings()
        {
            InitializeComponent();
        }

        private void SavedClicked(object sender, RoutedEventArgs e)
        {
            Debug.Print("Saved!!!!!!");
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
