using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.States
{
    internal class Loading : IState
    {
        Simulation simulation = new Simulation();

        bool loadingDone = false;

        string seed;
        float amp, freq, seaLevel, treeLine;
        int octaves;
        

        public Loading(string seed, float amp, float freq, int octaves, float seaLevel, float treeLine)
        {
            this.seed = seed;
            this.amp = amp; 
            this.freq = freq;
            this.octaves = octaves;
            this.seaLevel = seaLevel;
            this.treeLine = treeLine;
        }

        public IState Loop() 
        {
            if (loadingDone)
                return simulation;
            return this;
        }
        public void EnterState()
        {
            ClientCommunicator.RegisterData<bool>("LoadingDone", false);
            ClientCommunicator.RegisterData<string>("LoadingMessage", "Loading");
            Task.Run(() =>
            {
                simulation.CreateWorld(seed, amp, freq, octaves, seaLevel, treeLine);
				loadingDone = true;
				ClientCommunicator.UpdateData<bool>("LoadingDone", loadingDone);
			});
        }
        public void ExitState()
        {
        }
    }
}
