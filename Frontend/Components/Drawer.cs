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
    public class VectorPoint
    {
        public int x, y;
        public VectorPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class Drawer
    {
        int tileSize;
        Vector2Int mapPixelSize;

        Bitmap worldBitmap;
        Canvas Canvas;

        private Dictionary<string, Drawable> drawables;
        private Dictionary<(int, int), string> drawableGridLocations;

        public Drawer(ref Canvas mapCanvas)
        {
            Canvas = mapCanvas;
            drawables = new Dictionary<string, Drawable>();
            drawableGridLocations = new Dictionary<(int, int), string>();
        }

        public void Initialize()
        {
            tileSize = ClientCommunicator.GetData<int>("tileSize");
            mapPixelSize = ClientCommunicator.GetData<Vector2Int>("mapPixelSize");
            worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage");
            Debug.Print(String.Format("{0:N}", tileSize));
            if (tileSize != default(int) && mapPixelSize.X != default(int) && mapPixelSize.Y != default(int) && worldBitmap != default(Bitmap))
            {
                Debug.Print("Successfully loaded map values onto frontend. . .");
            }
        }

        public void checkMouseClick(int x, int y)
        {
            VectorPoint p = ConvertPixelsToGridPosition(x, y);

            if (drawableGridLocations.ContainsKey((p.x, p.y)))
            {
                Drawable temp;
                if (drawables.TryGetValue(drawableGridLocations[(p.x, p.y)], out temp))
                {
                    InfoModal im = new InfoModal(temp.image, temp.title, temp.getInformation());
                    im.Show();
                }
                else
                {
                    Debug.Print("Could not find image in drawables");
                }
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
                    if (!d.isMapOrGridlines)
                    {
                        Canvas.SetLeft(d.image, d.x);
                        Canvas.SetTop(d.image, d.y);
                    }
                    Canvas.Children.Add(d.image);
                }
            }
        }

        public void SetGridlines()
        {
            drawables["Grid"].shouldDraw = !drawables["Grid"].shouldDraw;
            Draw();
        }

        public void Draw()
        {
            ClearCanvas();
            if (drawables.Count == 0)
            {
                AddBitmapToCanvas("MainMap", worldBitmap);
                AddBitmapToCanvas("Grid", GetGridlines());
                AddImageToCanvas("TestSpriteRed_1", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\TestSpriteRed.png", 3, 2);
                AddImageToCanvas("TestSpriteYellow_1", $"{Properties.Resources.ProjectDir}\\Frontend\\Images\\Sprites\\TestSpriteYellow.png", 10, 15);
                drawables["Grid"].shouldDraw = false;
                drawables["Grid"].image.Opacity = 0.2;
            }
            PopulateCanvas();
        }

        public void AddBitmapToCanvas(string name, Bitmap worldBitmap, int x = 0, int y = 0)
        {
            Controls.Image img = new Controls.Image
            {
                Width = worldBitmap.Width,
                Height = worldBitmap.Height,
                Source = TransferToWriteableBitmap(worldBitmap)
            };
            if (!drawables.TryAdd(name, new Drawable(img, true, name)))
            {
                Debug.Print(String.Format("!!!ERROR: Cannot add {0:S} to drawables", name));
            }
        }

        public void AddImageToCanvas(string name, Controls.Image img)
        {

        }

        public void AddImageToCanvas(string name, string source, int x = 0, int y = 0)
        {
            // Used to add individual sprite images. Give relative path for source.

            Controls.Image img = new Controls.Image
            {
                Source = new BitmapImage(new Uri(source, UriKind.Absolute))
            };
            VectorPoint p = ConvertGridPositionToPixels(x, y);

            if (!drawables.TryAdd(name, new Drawable(img, true, name, source, p.x, p.y, x, y)))
            {
                Debug.Print(String.Format("!!!ERROR: Cannot add {0:S} to drawables", name));
            }

            if (!drawableGridLocations.TryAdd((x, y), name))
            {
                Debug.Print(String.Format("!!!ERROR: Cannot add {0:S} to drawableGridLocations", name));
            }
        }

        public VectorPoint ConvertGridPositionToPixels(int x, int y)
        {
            // based on current Sprite being 20x20 sprites
            int xPixelWidth = x * tileSize + (tileSize - 20);
            int yPixelWidth = y * tileSize + (tileSize - 20);
            return new VectorPoint(xPixelWidth, yPixelWidth);
        }

        public VectorPoint ConvertPixelsToGridPosition(int x, int y)
        {
            int xGrid = x / tileSize;
            int yGrid = y / tileSize;
            return new VectorPoint(xGrid, yGrid);
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