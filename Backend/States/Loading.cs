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
        public delegate void loadingTask(Simulation simulation, ref bool loadingDone);
        loadingTask task;
        bool loadingDone = false;

        public Loading(loadingTask task)
        {
            this.task = task;
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
                task(simulation, ref loadingDone);
            });
        }
        public void ExitState()
        {

        }
    }
}