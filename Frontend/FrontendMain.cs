

namespace JuniorProject.Frontend
{
	class FrontendMain
	{

		public void FrontendStart()
		{
			Console.WriteLine($"Front end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
			Thread.CurrentThread.Name = "Frontend";

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.Run(new MainWindow());


		}


	}


}
