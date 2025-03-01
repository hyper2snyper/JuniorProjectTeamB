using System.Drawing;
using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Frontend.Components;

namespace JuniorProject.Backend.WorldData
{
    public class World : Serializable
    {
        public TileMap map;
        public UnitManager unitManager = new UnitManager();
        List<Nation> nations = new List<Nation>();

        public event Action RedrawAction;
        public World()
        {
            ClientCommunicator.RegisterData<World>("World", this);
            ClientCommunicator.RegisterAction<string>("SaveWorld", SaveWorld);
            Unit.LoadUnitTemplates();
            Building.LoadBuildingTemplates();
        }

        public void GenerateWorld(int tileSize, Vector2Int mapPixelSize, string seed, float amp, float freq, int octaves, float seaLevel, float treeLine)
        {
            map = new TileMap(tileSize, mapPixelSize, seed, amp, freq, octaves, seaLevel, treeLine);
			ClientCommunicator.UpdateData<string>("LoadingMessage", "Placing Nations...", true);
			nations.Add(new Nation("Team Red", "Red", map, 0, this));
			nations.Add(new Nation("Team Green", "Green", map, 1, this));
			nations.Add(new Nation("Team Yellow", "Yellow", map, 2, this));
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
			foreach (Nation nation in nations)
			{
				nation.TakeTurn(tickCount);
			}
			unitManager.Update(tickCount);
            RedrawAction.Invoke();
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
            unitManager.LinkUnits(this.map);
            this.map.Map = map;
        }

        public void PopulateDrawablesList(ref List<GenericDrawable> genericDrawables) {
            Debug.Print("HELLLOOOO LEEEEEEEROY");
            foreach (Nation currentNation in nations)
            {
                foreach (TileMap.Tile currentTile in map.tiles) // maybe change this to nation.territory?
                {
                    if (currentTile.team != String.Empty)
                    {
                        genericDrawables.Add(new GenericDrawable(currentTile.pos, $"{currentTile.team}TileCover", 0));
                    }
                }
                foreach (Unit currentUnit in currentNation.units) {
                    genericDrawables.Add(new GenericDrawable(currentUnit.getPosition().pos, currentUnit.getSpriteName(), 1));   
                }
                foreach (Building currentBuilding in currentNation.buildings) {
                    genericDrawables.Add(new GenericDrawable(currentBuilding.gridPosition, currentBuilding.sprite, 1));
                }
            }
        }

        public override void SerializeFields()
        {
            SerializeField(unitManager);
            SerializeField(map);
        }

        public override void DeserializeFields()
        {
            unitManager = (UnitManager)DeserializeObject();
            map = (TileMap)DeserializeObject();
        }
    }

}