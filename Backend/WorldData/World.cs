using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World : Serializable
    {
        public TileMap map;
        public Dictionary<string, Nation> nations = new Dictionary<string, Nation>();


        public Action RedrawAction;
        
        
        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            ClientCommunicator.RegisterAction<string>("SaveWorld", SaveWorld);
            Map.LoadTerrain();
            Unit.LoadUnitTemplates();
            Building.LoadBuildingTemplates();
        }

        public void GenerateWorld(int tileSize, Vector2Int mapPixelSize, string seed, float amp, float freq, int octaves, float seaLevel, float treeLine)
        {
            map = new TileMap(tileSize, mapPixelSize, seed, amp, freq, octaves, seaLevel, treeLine);
			ClientCommunicator.UpdateData<string>("LoadingMessage", "Placing Nations...", true);
			nations.Add("Red", new Nation("Team Red", "Red", 0, this));
			nations.Add("Green", new Nation("Team Green", "Green", 1, this));
			nations.Add("Yellow", new Nation("Team Yellow", "Yellow", 2, this));
		}

        public void FreeWorld()
        {
            ClientCommunicator.UnregisterData("mapPixelSize");
            ClientCommunicator.UnregisterData("tileSize");
            ClientCommunicator.UnregisterData("World");
            ClientCommunicator.UnregisterData("UnitManager");
            ClientCommunicator.UnregisterData("TileMap");
            ClientCommunicator.UnregisterAction("SaveWorld");
        }

        public void Update(ulong tickCount)
        {
			foreach (Nation nation in nations.Values)
			{
				nation.TakeTurn(tickCount);
                RedrawAction?.Invoke();
			}
        }

        public void SaveWorld(string location)
        {
            Serializer serializer = new Serializer(location);
            map.SaveMapImage(serializer);
            serializer.SaveObject(this);
            serializer.Save();
        }

        public void LoadWorld(Map map)
        {
            this.map.Map = map;
        }

        public void PopulateDrawablesList(ref List<GenericDrawable> genericDrawables) {
            foreach (Nation currentNation in nations.Values)
            {
                currentNation.PopulateDrawablesList(ref genericDrawables);
            }
        }

        public Dictionary<string, Unit> GetAllUnits()
        {
            Dictionary<string, Unit> totalUnits = new Dictionary<string, Unit>();
            foreach(Nation currentNation in nations.Values)
            {
                foreach(Unit unit in currentNation.units)
                {
                    totalUnits.Add(unit.name, unit);
                }    
            }
            return totalUnits;
        }

        public override void SerializeFields()
        {
			SerializeField(map);
			SerializeField<string, Nation>(nations);
        }

        public override void DeserializeFields()
        {
            map = (TileMap)DeserializeObject();
            nations = DeserializeDictionary<string, Nation>((Nation n) =>
            {
                n.World = this;
            });
        }
    }

}