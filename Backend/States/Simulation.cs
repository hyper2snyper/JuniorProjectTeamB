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


        public void Loop()
        {

        }
        public void EnterState()
        {
            Debug.Print("Entered Simulation State.");
            world = new World();
        }
        public void ExitState() { }

    }
}
