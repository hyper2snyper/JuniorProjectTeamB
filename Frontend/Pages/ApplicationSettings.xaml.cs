using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace JuniorProject
{
    /// <summary>
    /// Interaction logic for ApplicationSettings.xaml
    /// </summary>
    public partial class ApplicationSettings : Page
    {
        public ApplicationSettings()
        {
            InitializeComponent();


        }
        public void ScreenSizeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            Window parentWindow = Window.GetWindow(this);
            if (radioButton != null && parentWindow != null)
            {
                switch (radioButton.Tag)
                {
                    case "Fullscreen":
                        parentWindow.WindowStyle = WindowStyle.None;
                        parentWindow.WindowState = WindowState.Maximized;
                        parentWindow.ResizeMode = ResizeMode.NoResize;
                        parentWindow.Topmost = true;
                        break;

                    case "1920x1080":
                        parentWindow.WindowState = WindowState.Normal;
                        parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                        parentWindow.ResizeMode = ResizeMode.CanResize;
                        parentWindow.Width = 1920;
                        parentWindow.Height = 1080;
                        break;
                    case "1280x720":
                        parentWindow.WindowState = WindowState.Normal;
                        parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                        parentWindow.ResizeMode = ResizeMode.CanResize;
                        parentWindow.Width = 1280;
                        parentWindow.Height = 720;
                        break;
                    case "900x600":
                        parentWindow.WindowState = WindowState.Normal;
                        parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                        parentWindow.ResizeMode = ResizeMode.CanResize;
                        parentWindow.Width = 900;
                        parentWindow.Height = 600;
                        break;
                }
            }
        }

        public void MusicPlayerValueChanged(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            var sliderAudio = sender as Slider;
            if (sliderAudio != null && parentWindow != null && parentWindow is MainWindow mainWindow)
            {
                mainWindow.GetMusicPlayer().Volume = sliderAudio.Value;
            }
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }
    }
}