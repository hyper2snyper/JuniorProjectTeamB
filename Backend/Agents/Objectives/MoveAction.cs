using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.Agents.Objectives
{
	class MoveAction : Objective
	{
		readonly Delegate? action = null;
		readonly TileMap.Tile target;
		List<TileMap.Tile> pathway;
		int pos = 0;

		public MoveAction(TileMap.Tile target, Delegate? action = null)
		{
			this.target = target;
			this.action = action;
		}

		public override Objective? PerformTurn(ulong tick)
		{
			if(pathway == null) 
			{
				pathway = Astar.FindPath(unit.tileMap, unit.pos, target);
				if(pathway.Count == 0) { return null; }
			}
			pos++;
			if (!unit.TryEnter(pathway[pos]))
			{
				//Recalculate path if possible.
			}
			unit.EnterTile(pathway[pos]);
			if(pos ==  pathway.Count-1)
			{
				action?.DynamicInvoke();
				return null;
			}
			return this;
		}
	}
}
