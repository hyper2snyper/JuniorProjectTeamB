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

    public partial class AccordionInfoModal : Window
    {
        public AccordionInfoModal(string TileTitle, string teamColor, List<Image> images, List<string> titles, List<string> information)
        {
            InitializeComponent();
            this.TileTitle.Text = TileTitle;

            if (!String.IsNullOrEmpty(teamColor)) {
                switch (teamColor)
                {
                    case "Yellow":
                        this.TileBorder.BorderBrush = new SolidColorBrush(Colors.Yellow);
                        break;
                    case "Green":
                        this.TileBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                        break;
                    case "Red":
                        this.TileBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < images.Count; i++) {
                AddExander(images[i], titles[i], information[i], i);
            }

        }

        void AddExander(Image image, string title, string information, int index) { 
            Expander expander = new Expander
            { 
                Header = title,
                IsExpanded = false,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.White)
            };

            Border expanderBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(3, 0, 3, 5)
            };

            expanderBorder.Child = expander;

            Grid contentGrid = new Grid();
            contentGrid.Margin = new Thickness(10);

            contentGrid.RowDefinitions.Add(new RowDefinition());
            contentGrid.RowDefinitions.Add(new RowDefinition());

            TextBlock textBlock = new TextBlock
            {
                Text = information,
                Margin = new Thickness (5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White)
            };

            Border textBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.DimGray),
                BorderThickness = new Thickness(2)
            };

            textBorder.Child = textBlock;

            Grid.SetRow(textBorder, 0);
            contentGrid.Children.Add(textBorder);

            Image currentImage = new Image
            {
                Source = image.Source,
                Width = 75,
                Height = 75
            };

            Border imageBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.DimGray),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 5, 0, 0),
                Height = 150,
                Width = 150
            };

            Grid.SetRow(imageBorder, 1);
            imageBorder.Child = currentImage;
            contentGrid.Children.Add(imageBorder);

            expander.Content = contentGrid;


            ExpanderGrid.Children.Add(expanderBorder);
        }
    }
}