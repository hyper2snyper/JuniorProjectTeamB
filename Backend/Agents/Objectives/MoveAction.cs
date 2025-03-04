using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.Agents.Objectives
{
	public class MoveAction : Objective
	{
		public delegate Objective? PostMoveAction(TileMap.Tile tile, Unit unit);
		readonly PostMoveAction? action = null;
		readonly TileMap.Tile target;
		List<TileMap.Tile> pathway;
		int pos = 0;

		public MoveAction(TileMap.Tile target, PostMoveAction? action = null)
		{
			this.target = target;
			this.action = action;
		}

		public override Objective? PerformTurn(ulong tick)
		{
			if(unit.pos == target)
			{
				return action?.Invoke(unit.pos, unit);
			}
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
				return action?.Invoke(unit.pos, unit); ;
			}
			return this;
		}
	}
}
