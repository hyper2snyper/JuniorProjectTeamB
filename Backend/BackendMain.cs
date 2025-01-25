using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JuniorProject.Backend
{
    internal class BackendMain
    {

        World mainWorld;
        bool running = true;

        public void BackendStart()
        {   
            Thread.CurrentThread.Name = "Backend";
            Debug.Print($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");
            mainWorld = new World();

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
            }


        }







    }
}
