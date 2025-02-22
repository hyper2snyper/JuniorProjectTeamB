using JuniorProject.Backend.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend
{
    public class DrawableManager
    {
        public Dictionary<string, Unit> units = new Dictionary<string, Unit>();
        //public List<string> units = new List<string>();
        public event Action DictionaryChanged;

        public DrawableManager() {
            ClientCommunicator.RegisterData<DrawableManager>("DrawableManager", this);
            //ClientCommunicator.RegisterAction("AddUnit", this.AddUnit);
            //ClientCommunicator.RegisterAction("RemoveUnit", this.RemoveUnit);
        }

        public void AddUnit(string name, Unit unit)
        { 
            units.TryAdd(name, unit);
            //units.Add(name);
            Debug.Print("Added Unit");
            DictionaryChanged?.Invoke();
        }

        //void RemoveUnit(string name)
        //{ 
        //    if (units.Remove(name))
        //        DictionaryChanged?.Invoke();
        //    Debug.Print("Removed Unit");
        //}
    }
}
