using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Backend.Agents.Objectives
{
    public class MoveAction : Objective
    {
        public delegate Objective? PostMoveAction(TileMap.Tile tile, Unit unit);
        public delegate Objective? FailMoveAction(TileMap.Tile tile, Unit unit, bool nopath);
        readonly PostMoveAction? action = null;
        readonly FailMoveAction? failAction = null;
        readonly TileMap.Tile target;
        List<TileMap.Tile> pathway;
        int pos = 0;

        public MoveAction(TileMap.Tile target, PostMoveAction? action = null, FailMoveAction? failAction = null)
        {
            this.target = target;
            this.action = action;
            this.failAction = failAction;
        }

        public override Objective? PerformTurn(ulong tick)
        {
            if (unit == null) return null;
            if (unit.pos == target)
            {
                return action?.Invoke(unit.pos, unit);
            }
            if (pathway == null)
            {
                pathway = Astar.FindPath(unit.tileMap, unit.pos, target, 
                    (TileMap.Tile current, TileMap.Tile target) => {
                        //No entering enemy territory.
                        return target.impassible; 
                    });
                if (pathway.Count == 0) 
                {
                    if (unit.nation == null) return null;
                    //Now we want to check ports.
                    foreach(Building building in unit.nation.buildings)
                    {
                        if(building.template.name != "Port") continue;
                        pathway = Astar.FindPath(unit.tileMap, unit.pos, building.pos, 
                            (TileMap.Tile current, TileMap.Tile target) => {
								return target.impassible; 
                            });
                        if (pathway.Count == 0) continue;
                        List<TileMap.Tile> secondHalf = Astar.FindPath(unit.tileMap, building.pos, target);
                        if (secondHalf.Count == 0) continue;
                        secondHalf.RemoveAt(0);
                        pathway.AddRange(secondHalf);
                        break;
                    }
                    if (pathway.Count == 0)
                    {
                        return failAction?.Invoke(unit.pos, unit, true);
                    }
                }
            }
            pos++;
            if (!unit.TryEnter(pathway[pos]))
            {
                return failAction?.Invoke(unit.pos, unit, false);
            }
            unit.EnterTile(pathway[pos]);
            if (pos == pathway.Count - 1)
            {
                return action?.Invoke(unit.pos, unit); ;
            }
            return this;
        }
    }
}
