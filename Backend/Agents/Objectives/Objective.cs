using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
    public abstract class Objective
    {
        public Unit unit;

        public virtual void Attach(Unit unit)
        {
            this.unit = unit;
        }

        public virtual Objective? PerformTurn(ulong tick)
        {
            return null;
        }

    }
}
