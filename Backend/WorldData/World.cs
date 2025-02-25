using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    class World
    {
        public TileMap map;
        public List<Unit> units = new List<Unit>();

        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            ClientCommunicator.RegisterAction("SaveWorld", SaveWorld);
            Unit.LoadUnitTemplates();
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
        }


        public void Update()
        {

        }


        public void SaveWorld()
        {

        }

        
        


    }

}