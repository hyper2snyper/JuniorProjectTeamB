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

namespace JuniorProject.Frontend.Components
{

    public partial class InfoModal : Window
    {
        public InfoModal(Image img, string title, string information)
        {
            InitializeComponent();
            this.Image.Source = img.Source;
            this.Title.Text = title;
            this.Information.Text = information;
        }

        public void setTeam(string team) {
            switch (team) {
                case "Yellow":
                    this.Title.Foreground = Brushes.Yellow;
                    this.ImageBorder.BorderBrush = new SolidColorBrush(Colors.Yellow);
                    break;
                case "Green":
                    this.Title.Foreground = Brushes.Green;
                    this.ImageBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                    break;
                case "Red":
                    this.Title.Foreground = Brushes.Red;
                    this.ImageBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    break;
            }
        }
    }
}