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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void StartSimulation(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new Simulation());
		}

		private void Catalog(object sender, RoutedEventArgs e)
		{

		}

		private void Settings(object sender, RoutedEventArgs e)
		{

		}
	}
}