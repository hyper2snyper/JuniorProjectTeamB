

using JuniorProject.Backend;

namespace JuniorProject.Frontend
{
    internal class FrontendMain
    {
        [System.STAThreadAttribute()]
        public void FrontendStart()
        {
            Console.WriteLine($"Front end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            Thread.CurrentThread.Name = "Frontend";

            JuniorProject.App app = new JuniorProject.App();
            app.InitializeComponent();
            app.Run();


            Console.WriteLine("Frontend Closed.");
            ClientCommunicator.CallAction("EndMainLoop");
        }


    }


}
