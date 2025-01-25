using JuniorProject.Backend.States;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JuniorProject.Backend
{
    class BackendMain
    {
        bool running = true;
        
        IState? currentState;
        public void SetState(IState? newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public void BackendStart()
        {
            Thread.CurrentThread.Name = "Backend";
            Debug.Print($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");

            ClientCommunicator.RegisterAction<IState>("SetState", SetState);

            MainLoop();
            Debug.Print("Backend closed.");
        }

        public void MainLoop()
        {
            ClientCommunicator.RegisterAction("EndMainLoop", () =>
            {
                running = false;
                Debug.Print("EndMainLoop was called.");
            }); //Register an action to be called by the frontend that sets running to false;
            Debug.Print("MainLoop Started.");
            while (running)
            {
                ClientCommunicator.ProcessActions();
                currentState?.Loop();
            }


        }







    }
}
