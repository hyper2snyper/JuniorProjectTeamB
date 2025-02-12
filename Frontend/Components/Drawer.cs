using System.Drawing;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using JuniorProject.Backend;
using System.Windows;
using System.Drawing.Drawing2D;
using JuniorProject.Backend.Helpers;
using Controls = System.Windows.Controls;
using Colors = System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;

namespace JuniorProject.Frontend.Components
{
    public class Drawer
    {
        int tileSize;
        Vector2Int mapPixelSize;

        Bitmap worldBitmap;
        Canvas Canvas;

        private Dictionary<string, Drawable> drawables;

        public Drawer(ref Canvas mapCanvas)
        {
            Canvas = mapCanvas;
            drawables = new Dictionary<string, Drawable>();
        }

        public void Initialize()
        {
            tileSize = ClientCommunicator.GetData<int>("tileSize");
            mapPixelSize = ClientCommunicator.GetData<Vector2Int>("mapPixelSize");
            worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage");

            if (tileSize != default(int) && mapPixelSize.X != default(int) && mapPixelSize.Y != default(int) && worldBitmap != default(Bitmap))
            {
                Debug.Print("Successfully loaded map values onto frontend. . .");
            }
        }

        public void ClearCanvas()
        {
            Canvas.Children.Clear();
        }

        public void PopulateCanvas()
        {
            foreach (Drawable d in drawables.Values)
            {
                if (d != null && d.shouldDraw)
                {
                    Canvas.Children.Add(d.image);
                }
            }
        }

        public void SetGridlines()
        {
            drawables["Grid"].shouldDraw = !drawables["Grid"].shouldDraw;
            Draw();
        }

        public void AddBitmapToCanvas(string name, Bitmap worldBitmap, int x = 0, int y = 0)
        {
            Controls.Image img = new Controls.Image
            {
                Width = worldBitmap.Width,
                Height = worldBitmap.Height,
                Source = TransferToWriteableBitmap(worldBitmap)
            };
            drawables.TryAdd(name, new Drawable(img, true));
        }

        public void AddImageToCanvas()
        {

        }

        public void Draw()
        {
            ClearCanvas();
            if (drawables.Count == 0)
            {
                AddBitmapToCanvas("MainMap", worldBitmap);
                AddBitmapToCanvas("Grid", GetGridlines());
                drawables["Grid"].shouldDraw = false;
                drawables["Grid"].image.Opacity = 0.2;
            }
            PopulateCanvas();
        }
        public WriteableBitmap TransferToWriteableBitmap(Bitmap worldBitmap)
        {
            WriteableBitmap map = new WriteableBitmap(worldBitmap.Width, worldBitmap.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[4 * (worldBitmap.Width * worldBitmap.Height)]; //pixel color buffer. each color is four bytes.
            for (int y = 0; y < worldBitmap.Width; y++)
            {
                for (int x = 0; x < worldBitmap.Height; x++)
                {
                    Drawing.Color c = worldBitmap.GetPixel(x, y);
                    int pos = ((y * worldBitmap.Width) + x) * 4;
                    pixels[pos] = c.B;
                    pixels[pos + 1] = c.G;
                    pixels[pos + 2] = c.R;
                    pixels[pos + 3] = c.A;
                }
            }
            map.WritePixels(new Int32Rect(0, 0, worldBitmap.Width, worldBitmap.Height), pixels, (worldBitmap.Width * 4), 0); //Update the bitmap
            return map;
        }

        public Bitmap GetGridlines()
        {
            Bitmap gridBitmap = new Bitmap(mapPixelSize.X, mapPixelSize.Y);

            using (Graphics g = Graphics.FromImage(gridBitmap))
            {
                g.Clear(Colors.Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                using (Colors.Pen gridPen = new Colors.Pen(Colors.Color.Black, 2))
                {
                    gridPen.Alignment = PenAlignment.Inset;
                    for (int y = tileSize; y < gridBitmap.Width; y += tileSize)
                    {
                        g.DrawLine(gridPen, 0, y, mapPixelSize.X, y);
                    }

                    for (int x = tileSize; x < gridBitmap.Height; x += tileSize)
                    {
                        g.DrawLine(gridPen, x, 0, x, mapPixelSize.Y);
                    }
                }
            }

            return gridBitmap;
        }
    }
}