
using System.Drawing;
using JuniorProject.Backend.Agents;

namespace JuniorProject.Backend.WorldData
{
    class World
    {
        Map map;
        List<Unit> units = new List<Unit>();

        public World()
        {
            map = new Map();
            ClientCommunicator.RegisterAction("RegenerateWorld", map.GenerateWorld);
            ClientCommunicator.RegisterAction("SaveWorld", SaveWorld);
            ClientCommunicator.RegisterData<Bitmap>("WorldImage", map.worldImage);
            ClientCommunicator.RegisterData<World>("World", map);
        }

        public void GenerateWorld()
        {
            map.GenerateWorld();
        }

        public void SaveWorld()
        {
            map.SaveImage();
            Serializer serializer = new Serializer("LocalData\\Savetest.chs");
            Debug.Print("Saving units...");
            foreach (Unit unit in units)
            {
                serializer.SaveObject(unit);
            }
            Debug.Print("Saving tiles...");
            map.SaveMap(serializer);

            Debug.Print("Writing to file...");
            serializer.Save();
            Debug.Print("Saved!");


            serializer.Load();
        }

    }

}
