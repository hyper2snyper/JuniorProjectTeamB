using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData.Managers
{
    public class TileManager
    {
        public Dictionary<Vector2Int, string> tiles = new Dictionary<Vector2Int, string> ();
        public event Action DictionaryChanged;

        public TileManager()
        {
            ClientCommunicator.RegisterData<TileManager>("TileManager", this);
        }

        public void AddTile(Vector2Int position, string team)
        {
            tiles.TryAdd(position, team);
            DictionaryChanged?.Invoke();
        }

        public void UpdateTile(Vector2Int position, string team)
        {
            tiles[position] = team;
            DictionaryChanged?.Invoke();
        }

        public void RemoveTile(Vector2Int position)
        {
            if (tiles.Remove(position))
                DictionaryChanged?.Invoke();
        }
    }
}
