using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents.Objectives
{
    class FollowAction : Objective
    {
        readonly Delegate? action = null;
        readonly Mob targetMob;
        TileMap.Tile target;
        List<TileMap.Tile> pathway;
        int pos = 0;

        public FollowAction(Mob targetMob, Delegate? action = null) {
            this.targetMob = targetMob;
            this.target = targetMob.pos;
            this.action = action;
        }

        public Boolean IsAdjacent() {
            Vector2Int difference = unit.pos.pos - targetMob.pos.pos;
            if (Math.Abs(difference.X) == 1 && Math.Abs(difference.Y) == 1) {
                return true;
            }
            return false;
        }

        public override Objective? PerformTurn(ulong tick)
        {
            if (target == null) return null;

            if (pathway == null || target != targetMob.pos)
            {
                Debug.Print($"Calculating New Position . . . New Target: [{targetMob.pos.pos.X}, {targetMob.pos.pos.Y}] | Old Target: [{target.pos.X}, {target.pos.Y}]");
                pos = 0;
                target = targetMob.pos;

                pathway = Astar.FindPath(unit.tileMap, unit.pos, target);
                if (pathway.Count == 0 || IsAdjacent()) { return null; }
            }
            pos++;

            if (pos >= pathway.Count) return null; // Weird issue where line 50 causes out of bounds error

            if (!unit.TryEnter(pathway[pos]))
            {
                //Recalculate path if possible.
            }

            unit.EnterTile(pathway[pos]);

            if (IsAdjacent())
            {
                action?.DynamicInvoke();
                return null;
            }
            return this;
        }
    }
}
