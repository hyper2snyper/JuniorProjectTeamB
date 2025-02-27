using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World : Serializable
    {
        public TileMap map;
        UnitManager unitManager;
        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            ClientCommunicator.RegisterAction<string>("SaveWorld", SaveWorld);
            Unit.LoadUnitTemplates();
            unitManager = new UnitManager();
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
            unitManager.LinkUnits(this.map);
            this.map.Map = map;
        }

        public override void SerializeFields()
        {
            SerializeField(unitManager);
            SerializeField(map);
        }

        public override void DeserializeFields()
        {
            unitManager = (UnitManager)DeserializeObject();
            map = (TileMap)DeserializeObject();
        }
    }

}