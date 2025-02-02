using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend
{
    internal interface IState
    {
        public IState Loop();
        public void EnterState();
        public void ExitState();
    }
}
