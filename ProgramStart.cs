using System.Threading;
using JuniorProject.Frontend;
using JuniorProject.Backend;

namespace JuniorProject
{
	static class ProgramStart
	{
		static void Main()
		{

			FrontendMain frontend = new FrontendMain();
			Thread frontendThread = new Thread(new ThreadStart(frontend.FrontendStart));


			BackendMain backend = new BackendMain();
			Thread backendThread = new Thread(new ThreadStart(backend.BackendStart));

			frontendThread.Start();
			backendThread.Start();

		}
	}
}