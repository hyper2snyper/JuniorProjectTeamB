using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World
    {
        public TileMap map;
        public List<Unit> units = new List<Unit>();
        DrawableManager drawableManager = new DrawableManager();

        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            Unit.LoadUnitTemplates();
            drawableManager = new DrawableManager();
        }

        public void GenerateWorld(int tileSize, Vector2Int mapPixelSize, string seed, float amp, float freq, int octaves, float seaLevel, float treeLine)
        {
            map = new TileMap(tileSize, mapPixelSize, seed, amp, freq, octaves, seaLevel, treeLine);
        }

        public void FreeWorld()
        {
            ClientCommunicator.UnregisterData("mapPixelSize");
            ClientCommunicator.UnregisterData("tileSize");
            ClientCommunicator.UnregisterData("World");
            ClientCommunicator.UnregisterData("DrawableManager");
        }


        public void Update()
        {

        }


        public void SaveWorld()
        {

        }





    }

}