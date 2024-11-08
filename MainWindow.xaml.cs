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

		WriteableBitmap map;
		Image mapImage;
		public MainWindow()
		{
			InitializeComponent();

			map = new WriteableBitmap(100, 100, 96, 96, PixelFormats.Bgr32, null);

			mapImage = this.Map;
			mapImage.Source = map;
		}

		private void RefreshClicked(object sender, RoutedEventArgs e)
		{
			DrawPixel(9, 9);
			
		}

		private void StartClicked(object sender, RoutedEventArgs e)
		{

		}

		private void PauseClicked(object sender, RoutedEventArgs e)
		{

		}

		public void DrawPixel(int row, int column)
		{
			try
			{
				// Reserve the back buffer for updates.
				map.Lock();

				unsafe
				{
					// Get a pointer to the back buffer.
					IntPtr pBackBuffer = map.BackBuffer;

					// Find the address of the pixel to draw.
					pBackBuffer += row * map.BackBufferStride;
					pBackBuffer += column * 4;

					// Compute the pixel's color.
					int color_data = 255 << 16; // R
					color_data |= 128 << 8;   // G
					color_data |= 255 << 0;   // B

					// Assign the color data to the pixel.
					*((int*)pBackBuffer) = color_data;
				}

				// Specify the area of the bitmap that changed.
				map.AddDirtyRect(new Int32Rect(column, row, 1, 1));
			}
			finally
			{
				// Release the back buffer and make it available for display.
				map.Unlock();
			}
		}
	}
}