using System.Drawing;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using JuniorProject.Backend;
using System.Windows;
using System.Drawing.Drawing2D;
using JuniorProject.Backend.Helpers;

namespace JuniorProject.Frontend.Components
{
    public class Drawer
    {
        int tileSize;
        Vector2Int mapPixelSize;
        public Drawer()
        {
            tileSize = ClientCommunicator.GetData<int>("tileSize");
            mapPixelSize = ClientCommunicator.GetData<Vector2Int>("mapPixelSize");
            Debug.Print(String.Format("Printing tileSize: {0:N}", tileSize));
        }

        public void Draw(Bitmap worldBitmap, ref WriteableBitmap map)
        {
            /* Layer everything onto given Bitmap from backend, then put it on WriteableBitmap */
            Debug.Print("Starting layering onto map...");
            Layer(worldBitmap, GetGridlines());
            TransferToWriteableBitmap(worldBitmap, ref map);
            Debug.Print("Done layering...");
        }
        public void TransferToWriteableBitmap(Bitmap worldBitmap, ref WriteableBitmap map)
        {
            byte[] pixels = new byte[4 * (worldBitmap.Width * worldBitmap.Height)]; //pixel color buffer. each color is four bytes.
            for (int y = 0; y < worldBitmap.Width; y++)
            {
                for (int x = 0; x < worldBitmap.Height; x++)
                {
                    Color c = worldBitmap.GetPixel(x, y);
                    int pos = ((y * worldBitmap.Width) + x) * 4;
                    pixels[pos] = c.B;
                    pixels[pos + 1] = c.G;
                    pixels[pos + 2] = c.R;
                    pixels[pos + 3] = c.A;
                }
            }
            map.WritePixels(new Int32Rect(0, 0, worldBitmap.Width, worldBitmap.Height), pixels, (worldBitmap.Width * 4), 0); //Update the bitmap
        }
        public void Layer(Bitmap worldBitmap, Bitmap layerMap)
        {
            for (int y = 0; y < mapPixelSize.Y; y++)
            {
                for (int x = 0; x < mapPixelSize.X; x++)
                {
                    if (layerMap.GetPixel(x, y).A > 0)
                    {
                        worldBitmap.SetPixel(x, y, layerMap.GetPixel(x, y));
                    }
                }
            }
        }

        public void Layer(WriteableBitmap layer, ref WriteableBitmap map)
        {
        }

        public Bitmap GetGridlines()
        {
            Bitmap gridBitmap = new Bitmap(mapPixelSize.X, mapPixelSize.Y);

            using (Graphics g = Graphics.FromImage(gridBitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                using (Pen gridPen = new Pen(Color.Black, 5))
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
