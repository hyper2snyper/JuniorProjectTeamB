using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;


namespace JuniorProject.Backend.States
{
    internal class Simulation : IState
    {
        World world;
        bool paused = true;

        ulong lastTick;
        ulong timePerTick = (10000) * 100; //Ticks are in units of 100 nanoseconds. 10,000 in a millisecond.
        ulong tickCount = 0;


        public void CreateWorld(string seed, float freq, float amp, int octaves, float seaLevel, float treeLine)
        {
            world = new World();
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating World");
            world.GenerateWorld(20, new Vector2Int(1000,1000), seed, freq, amp, octaves, seaLevel, treeLine);
		}

        public IState Loop()
        {
            if (paused) return this;

            ulong currentTick = (uint)DateTime.Now.Ticks;
            if ((lastTick + timePerTick) > currentTick) return this;
            Debug.RePrint($"Tick count [{tickCount}]");
            tickCount++;
            lastTick = currentTick;
            world.Update();
            return this;
        }
        public void EnterState()
        {
            Debug.Print("Entered Simulation State.");
            lastTick = (uint)DateTime.Now.Ticks;
            ClientCommunicator.RegisterAction("TogglePause", () => { paused = !paused; });
        }
         
        public void ExitState() 
        {
            world.FreeWorld();
            ClientCommunicator.UnregisterAction("TogglePause");
        }

    }
}
