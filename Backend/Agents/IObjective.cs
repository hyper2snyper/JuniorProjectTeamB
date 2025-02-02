using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
	internal interface IObjective
	{
		Unit unit { get; set; }

		public void ObjectiveStart();
		public void ObjectiveStop();
		
		public void PerformTurn();

	}
}
