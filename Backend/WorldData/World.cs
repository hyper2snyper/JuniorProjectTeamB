using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData.Managers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World
    {
        public TileMap map;
        UnitManager unitManager;
        TileManager tileManager;
        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            Unit.LoadUnitTemplates();
            unitManager = new UnitManager();
            tileManager = new TileManager();
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
            ClientCommunicator.UnregisterData("UnitManager");
            ClientCommunicator.UnregisterData("TileManager");
            ClientCommunicator.UnregisterData("Tiles");
        }


        public void Update()
        {

        }


        public void SaveWorld()
        {

        }





    }

}