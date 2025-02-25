using JuniorProject.Backend.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData.Managers
{
    public class UnitManager
    {
        public Dictionary<string, Unit> units = new Dictionary<string, Unit>();
        public event Action DictionaryChanged;

        public UnitManager()
        {
            ClientCommunicator.RegisterData<UnitManager>("UnitManager", this);
        }

        public void AddUnit(string name, Unit unit)
        {
            units.TryAdd(name, unit);
            DictionaryChanged?.Invoke();
        }

        public void RemoveUnit(string name)
        {
            if (units.Remove(name))
                DictionaryChanged?.Invoke();
        }
    }
}
