
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.States
{
    internal class Simulation : IState
    {
        World world;
        bool paused = true;

        ulong lastTick;
        ulong timePerTick = (10000)*100; //Ticks are in units of 100 nanoseconds. 10,000 in a millisecond.
        ulong tickCount = 0;


        public void CreateWorld()
        {
			world = new World();
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating World");
            world.GenerateWorld();
		}

        public IState Loop()
        {
			if (paused) return this;

			ulong currentTick = (uint)DateTime.Now.Ticks;
            if ((lastTick + timePerTick) > currentTick) return this;
            Debug.RePrint($"Tick count [{tickCount}]");
            tickCount++;
            lastTick = currentTick;

            return this;
        }
        public void EnterState()
        {
			Debug.Print("Entered Simulation State.");
            lastTick = (uint)DateTime.Now.Ticks;
            ClientCommunicator.RegisterAction("TogglePause", () => { paused = !paused; });
        }
         
        public void ExitState() { }

    }
}
