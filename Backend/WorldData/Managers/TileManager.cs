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
        public Dictionary<(int, int), string> tiles = new Dictionary<(int, int), string>();
        public event Action DictionaryChanged;

        public TileManager()
        {
            ClientCommunicator.RegisterData<TileManager>("TileManager", this);
        }

        public void AddTile(Vector2Int position, string team)
        {
            tiles.TryAdd((position.X, position.Y), team);
            DictionaryChanged?.Invoke();
        }

        public void UpdateTile(Vector2Int position, string team)
        {
            tiles[(position.X, position.Y)] = team;
            DictionaryChanged?.Invoke();
        }

        public void RemoveTile(Vector2Int position)
        {
            if (tiles.Remove((position.X, position.Y)))
                DictionaryChanged?.Invoke();
        }
    }
}
