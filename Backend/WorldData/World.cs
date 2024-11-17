
using System.Drawing;

namespace JuniorProject.Backend.WorldData
{
	class World
	{
		Map map;

		public World()
		{
			map = new Map();
			map.GenerateWorld(0.0008f, 5f);
		}

	}

}
