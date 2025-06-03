using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Helpers
{
    internal class Astar
    {
        public delegate bool TileFilter(TileMap.Tile current, TileMap.Tile target);

        public static float CostCalc(TileMap.Tile tile, TileMap.Tile target)
        {
            return (target.pos - tile.pos).Magnitude;
        }

        public static List<TileMap.Tile> FindPath(TileMap map, TileMap.Tile start, TileMap.Tile target, TileFilter? filter = null)
        {
            PriorityQueue<TileMap.Tile, float> openSet = new PriorityQueue<TileMap.Tile, float>();
            openSet.Enqueue(start, 0);

            Dictionary<TileMap.Tile, TileMap.Tile> cameFrom = new Dictionary<TileMap.Tile, TileMap.Tile>();

            Dictionary<TileMap.Tile, float> gScore = new Dictionary<TileMap.Tile, float>();
            gScore[start] = 0;

            Dictionary<TileMap.Tile, float> fScore = new Dictionary<TileMap.Tile, float>();
            fScore[start] = CostCalc(start, target);

            while (openSet.Count > 0)
            {
                TileMap.Tile current = openSet.Dequeue();
                if (current == target)
                {
                    List<TileMap.Tile> totalPath = [current];
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        totalPath = totalPath.Prepend(current).ToList();
                    }
                    return totalPath; //Success
                }

                foreach (TileMap.Tile? neighbor in map.getTileNeighbors(current))
                {
                    if (neighbor == null) continue;
                    if (filter != null && filter(current, neighbor)) continue;
                    float tentativeGscore = gScore[current] + neighbor.movementCost;
                    float neighborGscore = float.PositiveInfinity;
                    if (gScore.ContainsKey(neighbor))
                    {
                        neighborGscore = gScore[neighbor];
                    }
                    if (tentativeGscore < neighborGscore)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGscore;
                        fScore[neighbor] = tentativeGscore + CostCalc(neighbor, target);

                        if (!openSet.UnorderedItems.Contains((neighbor, fScore[neighbor])))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }
            return new List<TileMap.Tile>();
        }



    }
}
