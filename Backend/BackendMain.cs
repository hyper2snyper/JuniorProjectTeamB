using JuniorProject.Backend.Agents;
using JuniorProject.Backend.States;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JuniorProject.Backend.Agents.Building;


namespace JuniorProject.Backend
{
    class BackendMain
    {
        bool running = true;

        IState? currentState;
        public void SetState(IState? newState)
        {
            if (newState == null) return;
            if (newState == currentState) return;
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public void BackendStart()
        {
            Thread.CurrentThread.Name = "Backend";
            Debug.Print($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");


            // === TEMP TEST FOR Building.TakeTurn ===
            var nation = new Nation();
            var farmTemplate = new BuildingTemplate { name = "Farm" };
            var building = new Building
            {
                template = farmTemplate,
                nation = nation
            };

            var nation1 = new Nation();
            var mineTemplate = new BuildingTemplate { name = "Mine" };
            var building2 = new Building
            {
                template = mineTemplate,
                nation = nation1
            };

            building.TakeTurn(0);
            building2.TakeTurn(0);
            Debug.Print($"[TEST] Nation money after TakeTurn (Farm): {nation.money}");
            Debug.Print($"[TEST] Nation money after TakeTurn (Mine): {nation1.money}");
            var db = new Database("LocalData/BackendDatabase.db");


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
                SetState(currentState?.Loop());
            }


        }







    }
}
