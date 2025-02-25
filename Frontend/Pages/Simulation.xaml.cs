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
using JuniorProject.Frontend.Windows;

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

        private System.Windows.Point _startPanning;
        private System.Windows.Point _startClicking;
        private double _zoomFactor = 1.1;
        private bool _isDragging = false;
        private double _clickAbsoluteX = 0;
        private double _clickAbsoluteY = 0;
        private ScaleTransform _scaleTransform = new ScaleTransform(1, 1);
        private TranslateTransform _translateTransform = new TranslateTransform();

        private string relativePath = "LocalData\\Map.png";
        public Simulation()
        {
            InitializeComponent();
            mapCanvas = this.Canvas;
            drawer = new Drawer(ref mapCanvas);

            //mapCanvas.PreviewMouseWheel += CanvasMouseWheel; disabled for now
            mapCanvas.MouseLeftButtonDown += CanvasMouseLeftDown;
            mapCanvas.MouseLeftButtonUp += CanvasMouseLeftUp;
            mapCanvas.MouseMove += CanvasMouseMove;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(_scaleTransform);
            transformGroup.Children.Add(_translateTransform);
            mapCanvas.RenderTransform = transformGroup;
            drawer.Initialize();
            drawer.Draw();



#if DEBUG
            KeyDown += OnKeyDown;
#endif
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemTilde)
            {
                DebugWindow.ShowWindow(this);
            }
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {

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
            var fileDialouge = new Microsoft.Win32.SaveFileDialog();
            fileDialouge.DefaultDirectory = "Save";
            fileDialouge.DefaultExt = ".chs";
			fileDialouge.Filter = "Cry Havoc Save (.chs)|*.chs";
            if(fileDialouge.ShowDialog() != true)
            {
                return;
            }
			ClientCommunicator.CallAction("SaveWorld", fileDialouge.FileName);
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Disabled for now

            //System.Windows.Point mousePosition = e.GetPosition(mapCanvas);
            //double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            //_scaleTransform.ScaleX *= zoomFactor;
            //_scaleTransform.ScaleY *= zoomFactor;
            ////Debug.Print(String.Format("Mouse Pos: {0:N}, {1:N}", mousePosition.X, mousePosition.Y));
            ////Debug.Print(String.Format("Zoom Factor: {0:N}, {1:N}", _scaleTransform.ScaleX, _scaleTransform.ScaleY));

            //var newScaleX = _scaleTransform.ScaleX;
            //var newScaleY = _scaleTransform.ScaleY;

            //double offsetX = mousePosition.X * (1 - zoomFactor);
            //double offsetY = mousePosition.Y * (1 - zoomFactor);

            //_translateTransform.X += offsetX;
            //_translateTransform.Y += offsetY;
        }

        private void CanvasMouseLeftDown(object sender, MouseEventArgs e)
        {
            _isDragging = true;
            _startPanning = e.GetPosition(ScrollViewer);
            _startClicking = e.GetPosition(ScrollViewer);
            mapCanvas.CaptureMouse();
        }

        private void CanvasMouseLeftUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;

            System.Windows.Point currentPosition = e.GetPosition(ScrollViewer);
            double offsetX = currentPosition.X - _startClicking.X;
            double offsetY = currentPosition.Y - _startClicking.Y;

            if ((Math.Abs(offsetX) < 1 && Math.Abs(offsetY) < 1))
            {
                Debug.Print(String.Format("COMBINED: Clicked mouse at: {0:N}, {1:N}", (int)(currentPosition.X - _clickAbsoluteX), (int)(currentPosition.Y - _clickAbsoluteY)));
                drawer.checkMouseClick((int)(currentPosition.X - _clickAbsoluteX), (int)(currentPosition.Y - _clickAbsoluteY));
            }

            mapCanvas.ReleaseMouseCapture();
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point currentPosition = e.GetPosition(ScrollViewer);
            if (_isDragging)
            {
                double offsetX = currentPosition.X - _startPanning.X;
                double offsetY = currentPosition.Y - _startPanning.Y;

                if (Math.Abs(offsetX) > 1 || Math.Abs(offsetY) > 1)
                {
                    var currentMargin = mapCanvas.Margin;
                    _clickAbsoluteX = currentMargin.Left + offsetX;
                    _clickAbsoluteY = currentMargin.Top + offsetY;
                    mapCanvas.Margin = new Thickness(currentMargin.Left + offsetX, currentMargin.Top + offsetY, 0, 0);
                    _startPanning = currentPosition;
                }
            }

            Coords.Content = $"({(int)e.GetPosition(Canvas).X / drawer.tileSize},{(int)e.GetPosition(Canvas).Y / drawer.tileSize})";
        }

        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            SimulationSettings simSettings = new SimulationSettings();
            simSettings.Show();
            Debug.Print("SimSettings Windows");
        }
    }
}