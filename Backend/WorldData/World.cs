
using System.Drawing;

namespace JuniorProject.Backend.WorldData
{
    class World
    {
        Map map;

        public World()
        {
            map = new Map();
            ClientCommunicator.RegisterAction("RegenerateWorld", map.GenerateWorld);
            ClientCommunicator.RegisterAction("SaveWorld", map.SaveImage);
            ClientCommunicator.RegisterData<Bitmap>("WorldImage", map.worldImage);
            ClientCommunicator.RegisterData<World>("World", map);
        }

    }

}
