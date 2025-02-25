using JuniorProject.Backend.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.WorldData
{
    public class UnitManager : Serializable
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

        public void LinkUnits(TileMap map)
        {
			ClientCommunicator.UpdateData<string>("LoadingMessage", "Linking Units");
			foreach (Unit unit in units.Values)
            {
                unit.setPosition(map.getTile(unit.getPosition()));
            }
        }

		public override void SerializeFields()
		{
            SerializeField(units);
		}

		public override void DeserializeFields()
		{
            units = DeserializeDictionary<string, Unit>();
		}
	}
}
