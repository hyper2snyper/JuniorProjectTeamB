

using JuniorProject.Backend;

namespace JuniorProject.Frontend
{
    internal class FrontendMain
    {
        [System.STAThreadAttribute()]
        public void FrontendStart()
        {
			Thread.CurrentThread.Name = "Frontend";
			Debug.Print($"Front end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
            

            JuniorProject.App app = new JuniorProject.App();
            app.InitializeComponent();
            app.Run();


			Debug.Print("Frontend Closed.");
            ClientCommunicator.CallAction("EndMainLoop");
        }


    }


}
