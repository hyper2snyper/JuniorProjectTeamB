using JuniorProject.Backend.WorldData;
using JuniorProject.Backend.Helpers;

namespace JuniorProject.Backend.Agents.Objectives
{
	public class Sail : Objective
	{

		TileMap.Tile target;
		
		public Sail(TileMap.Tile target) 
		{
			this.target = target;
		}

		public override void Attach(Unit unit)
		{
			base.Attach(unit);
			unit.embarked = true;
		}

		public override Objective? PerformTurn(ulong tick)
		{
			Vector2Int dir = new Vector2Int(Math.Clamp(target.pos.X-unit.pos.pos.X, -1, 1), Math.Clamp(target.pos.Y - unit.pos.pos.Y, -1, 1));
			unit.EnterTile(unit.tileMap.getTile(unit.pos.pos+dir));
			if(unit.pos == target)
			{
				unit.embarked = false;
				return null;
			}
			return this;
		}


	}
}
