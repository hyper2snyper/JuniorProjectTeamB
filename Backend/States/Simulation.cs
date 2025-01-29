using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.States
{
    internal class Simulation : IState
    {
        World world;
        const int TILE_SIZE = 20;


        public void Loop()
        {

        }
        public void EnterState()
        {
            Debug.Print("Entered Simulation State.");
            Debug.Print("Registering TILE_SIZE into Client Communicator...");
            ClientCommunicator.RegisterData<int>("TILE_SIZE", TILE_SIZE);
            world = new World();
        }
        public void ExitState() { }

    }
}
