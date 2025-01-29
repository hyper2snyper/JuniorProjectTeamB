using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using JuniorProject.Backend;
using System.Windows;
using System.Windows.Media;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Brushes = System.Windows.Media.Brushes;
using System;
using System.Drawing; // For Bitmap, Color, etc.
using System.Drawing.Imaging; // For ImageFormat
using System.IO; // For MemoryStream
using System.Windows.Media.Imaging; // 

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

        public void Draw(Drawing.Bitmap worldBitmap, ref WriteableBitmap map)
        {
            Debug.Print("Drawing onto map...");
            Layer(worldBitmap, ref map);
            Layer(GetGridlines(), ref map);
        }

        public void Layer(Drawing.Bitmap worldBitmap, ref WriteableBitmap map) 
        {
            /* Used to draw the initial map given from the backend */
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
        }

        public void Layer(WriteableBitmap layer, ref WriteableBitmap map)
        {
            for (int y = 0; y < MAP_PIXEL_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_PIXEL_WIDTH; x++) 
                {
                }
            }
        }

        public WriteableBitmap GetGridlines()
        {
            WriteableBitmap gridBitmap = new WriteableBitmap(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                Pen gridPen = new Pen(Brushes.Black, 1);

                for (int x = TILE_SIZE; x < MAP_PIXEL_WIDTH / TILE_SIZE; x += TILE_SIZE)
                {
                    drawingContext.DrawLine(gridPen, new Point(x, 0), new Point(x, MAP_PIXEL_HEIGHT));
                }
                for (int y = TILE_SIZE; y < MAP_PIXEL_HEIGHT / TILE_SIZE; y += TILE_SIZE) 
                { 
                    drawingContext.DrawLine(gridPen, new Point(0, y), new Point(MAP_PIXEL_WIDTH, y));
                }
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap(MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            renderTarget.CopyPixels(new Int32Rect(0, 0, MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT), gridBitmap.BackBuffer, gridBitmap.BackBufferStride * MAP_PIXEL_HEIGHT, gridBitmap.BackBufferStride);
            gridBitmap.AddDirtyRect(new Int32Rect(0, 0, MAP_PIXEL_WIDTH, MAP_PIXEL_HEIGHT));

            return gridBitmap;
        }
    }
}
