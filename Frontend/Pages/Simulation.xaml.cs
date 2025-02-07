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

        private System.Windows.Point _start;
        private double _zoomFactor = 1.1;
        private bool _isDragging = false;
        private ScaleTransform _scaleTransform = new ScaleTransform(1, 1);
        private TranslateTransform _translateTransform = new TranslateTransform();

        private string relativePath = "LocalData\\Map.png";
        public Simulation()
        {
            InitializeComponent();
            mapCanvas = this.Canvas;
            drawer = new Drawer(ref mapCanvas);

            mapCanvas.PreviewMouseWheel += CanvasMouseWheel;
            mapCanvas.MouseLeftButtonDown += CanvasMouseLeftDown;
            mapCanvas.MouseLeftButtonUp += CanvasMouseLeftUp;
            mapCanvas.MouseMove += CanvasMouseMove;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(_scaleTransform);
            transformGroup.Children.Add(_translateTransform);
            mapCanvas.RenderTransform = transformGroup;

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

        private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point mousePosition = e.GetPosition(mapCanvas);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;
            //Debug.Print(String.Format("Mouse Pos: {0:N}, {1:N}", mousePosition.X, mousePosition.Y));
            //Debug.Print(String.Format("Zoom Factor: {0:N}, {1:N}", _scaleTransform.ScaleX, _scaleTransform.ScaleY));

            var newScaleX = _scaleTransform.ScaleX;
            var newScaleY = _scaleTransform.ScaleY;

            double offsetX = mousePosition.X * (1 - zoomFactor);
            double offsetY = mousePosition.Y * (1 - zoomFactor);

            _translateTransform.X += offsetX;
            _translateTransform.Y += offsetY;
        }

        private void CanvasMouseLeftDown(object sender, MouseEventArgs e)
        {
            _isDragging = true;
            _start = e.GetPosition(ScrollViewer);
            mapCanvas.CaptureMouse();
        }

        private void CanvasMouseLeftUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            mapCanvas.ReleaseMouseCapture();
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                //Debug.Print("Attempting panning");
                System.Windows.Point currentPosition = e.GetPosition(ScrollViewer);
                double offsetX = currentPosition.X - _start.X;
                double offsetY = currentPosition.Y - _start.Y;

                if (Math.Abs(offsetX) > 1 || Math.Abs(offsetY) > 1)
                {
                    var currentMargin = mapCanvas.Margin;
                    mapCanvas.Margin = new Thickness(currentMargin.Left + offsetX, currentMargin.Top + offsetY, 0, 0);
                    _start = currentPosition;
                }
            }
        }
    }
}