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
                simulation.CreateWorld();
				loadingDone = true;
				ClientCommunicator.UpdateData<bool>("LoadingDone", loadingDone);
			});
        }
        public void ExitState() 
        {
        }
    }
}
