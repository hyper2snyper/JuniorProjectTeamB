using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData.Managers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World : Serializable
    {
        public TileMap map;
        DrawableManager drawableManager;
        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            ClientCommunicator.RegisterAction<string>("SaveWorld", SaveWorld);
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
            ClientCommunicator.UnregisterData("TileMap");
            ClientCommunicator.UnregisterAction("SaveWorld");
        }


        public void Update()
        {

        }

        public void SaveWorld(string location)
        {
            Serializer serializer = new Serializer(location);
            map.SaveMapImage(serializer);
            serializer.SaveObject(this);
            serializer.Save();
        }

        public void LoadWorld(Map map)
        {
            drawableManager.LinkUnits(this.map);
            this.map.Map = map;
        }

        public override void SerializeFields()
        {
            SerializeField(drawableManager);
            SerializeField(map);
        }

        public override void DeserializeFields()
        {
            drawableManager = (DrawableManager)DeserializeObject();
            map = (TileMap)DeserializeObject();
        }
    }

}