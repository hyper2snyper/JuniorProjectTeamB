using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData.Managers
{
    public class DrawableManager
    {
        public Dictionary<(int, int), string> drawables = new Dictionary<(int, int), string>();
        public event Action DictionaryChanged;

        public DrawableManager()
        {
            ClientCommunicator.RegisterData<DrawableManager>("DrawableManager", this);
        }

        public void AddUnit(Vector2Int gridPosition, string sprite)
        {
            if (drawables.TryAdd((gridPosition.X, gridPosition.Y), sprite))
                DictionaryChanged?.Invoke();
        }

        public void RemoveUnit(Vector2Int gridPosition)
        {
            if (drawables.Remove((gridPosition.X, gridPosition.Y)))
                DictionaryChanged?.Invoke();
        }
    }
}
