using JuniorProject.Backend;
using JuniorProject.Frontend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject
{
    static class ProgramMain
    {
        static void Main()
        {
            FrontendMain frontend = new FrontendMain();
            Thread frontendThread = new Thread(new ThreadStart(frontend.FrontendStart));
            frontendThread.SetApartmentState(ApartmentState.STA);


            BackendMain backend = new BackendMain();
            Thread backendThread = new Thread(new ThreadStart(backend.BackendStart));

            Debug.SetThreads(backendThread.ManagedThreadId, frontendThread.ManagedThreadId);

            frontendThread.Start();
            backendThread.Start();

        }
    }
}
