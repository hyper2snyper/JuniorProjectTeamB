using JuniorProject.Backend;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;

namespace JuniorProject
{
    public class SimulationService
    {
        private static SimulationService _instance;
        public static SimulationService Instance => _instance ??= new SimulationService();

        private Image mapImage;
        private string relativePath = "LocalData\\Map.png";

        public bool IsRunning { get; private set; }
        public TimeSpan SimulationTime { get; private set; }

        private SimulationService()
        {
            IsRunning = false;
            SimulationTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Sets the Image UI control so that the SimulationService can update it.
        /// This must be called from the UI before starting the simulation.
        /// </summary>
        public void SetMapImage(Image uiImage)
        {
            mapImage = uiImage;
        }

        public void StartSimulation()
        {
            IsRunning = true;
            if (mapImage != null)
            {
                mapImage.Source = ReloadImage();
            }
            Console.WriteLine("Simulation started.");

        }

        public void PauseSimulation()
        {
            IsRunning = false;
            Console.WriteLine("Simulation paused.");
        }

        public void SaveSimulation()
        {
            Console.WriteLine("Simulation state saved.");
        }

        public void UpdateSimulationTime(TimeSpan time)
        {
            SimulationTime = time;
            Console.WriteLine($"Simulation time updated: {SimulationTime}");
        }

        public void RefreshSimulation()
        {
            if (mapImage != null)
            {
                mapImage.Source = ReloadImage();
            }
            Console.WriteLine("Simulation refreshed.");
        }

        private WriteableBitmap ReloadImage()
        {
            ClientCommunicator.CallActionWaitFor("RegenerateWorld");

            Drawing.Bitmap worldBitmap = ClientCommunicator.GetData<Drawing.Bitmap>("WorldImage");
            if (worldBitmap == null)
            {
                Console.WriteLine("Failed to retrieve world image from backend.");
                return null;
            }

            WriteableBitmap writeableBitmap = new WriteableBitmap(
                worldBitmap.Width, worldBitmap.Height, 96, 96, PixelFormats.Bgra32, null
            );

            byte[] pixels = new byte[4 * (worldBitmap.Width * worldBitmap.Height)];
            for (int y = 0; y < worldBitmap.Height; y++)
            {
                for (int x = 0; x < worldBitmap.Width; x++)
                {
                    Drawing.Color c = worldBitmap.GetPixel(x, y);
                    int pos = ((y * worldBitmap.Width) + x) * 4;
                    pixels[pos] = c.B;
                    pixels[pos + 1] = c.G;
                    pixels[pos + 2] = c.R;
                    pixels[pos + 3] = c.A;
                }
            }

            writeableBitmap.WritePixels(
                new Int32Rect(0, 0, worldBitmap.Width, worldBitmap.Height),
                pixels, (worldBitmap.Width * 4), 0
            );

            return writeableBitmap;
        }
    }

}
