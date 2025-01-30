using System.Drawing;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using JuniorProject.Backend;
using System.Windows;
using System.Drawing.Drawing2D;

namespace JuniorProject.Frontend.Components
{
    public class Drawer
    {
        private int TILE_SIZE;
        const int MAP_PIXEL_WIDTH = 2000;
        const int MAP_PIXEL_HEIGHT = 2000;
        public Drawer()
        {
            TILE_SIZE = ClientCommunicator.GetData<int>("TILE_SIZE");
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
            for (int y = 0; y < MAP_PIXEL_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_PIXEL_WIDTH; x++)
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
            Bitmap gridBitmap = new Bitmap(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT);

            using (Graphics g = Graphics.FromImage(gridBitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                using (Pen gridPen = new Pen(Color.Black, 5))
                {
                    gridPen.Alignment = PenAlignment.Inset;
                    for (int y = TILE_SIZE; y < gridBitmap.Width; y += TILE_SIZE)
                    {
                        g.DrawLine(gridPen, 0, y, MAP_PIXEL_WIDTH, y);
                    }

                    for (int x = TILE_SIZE; x < gridBitmap.Height; x += TILE_SIZE)
                    {
                        g.DrawLine(gridPen, x, 0, x, MAP_PIXEL_HEIGHT);
                    }
                }
            }

            return gridBitmap;
        }
    }
}
