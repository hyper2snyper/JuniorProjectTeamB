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

            Console.WriteLine($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            Thread.CurrentThread.Name = "backend";
            DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");
            mainWorld = new World();

            MainLoop();
        }

        public void MainLoop()
        {
            ClientCommunicator.RegisterAction("EndMainLoop", () =>
            {
                running = false;
                Console.WriteLine("EndMainLoop was called.");
            }); //Register an action to be called by the frontend that sets running to false;
            Console.WriteLine("MainLoop Started.");
            while (running)
            {
                ClientCommunicator.ProcessActions();
            }


        }







    }
}
