using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Frontend.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend.Agents
{
	public abstract class Mob : Serializable
	{
		public TileMap? tileMap;
		public TileMap.Tile? pos;
		public Vector2Int PosVector
		{
			get
			{
				if(pos == null) return Vector2Int.Zero;
				return pos.pos;
			}
		}
		public Nation? nation;
		public string sprite;
		public GenericDrawable.DrawableType drawableType;

		public Mob() { }

		public Mob(TileMap tileMap, TileMap.Tile pos, Nation? nation)
		{
			this.nation = nation;
			this.tileMap = tileMap;
            drawableType = GenericDrawable.DrawableType.Mob;
			EnterTile(pos);
		}

		public virtual void TakeTurn(ulong tick) { }


		public virtual bool TryEnter(TileMap.Tile tile) { return true; }
		
		public virtual void EnterTile(TileMap.Tile tile) 
		{
			ExitTile(pos);
			pos = tile;
			tileMap.TilesUpdated();
			tile.Occupants.Add(this);
		}

		public virtual void ExitTile(TileMap.Tile tile) 
		{
			tile?.Occupants.Remove(this);
		}

		public virtual void populateDrawables(ref List<GenericDrawable> genericDrawables)
		{
			genericDrawables.Add(new GenericDrawable(PosVector, GetSprite(), drawableType));
		}

		public virtual string GetSprite()
		{
			return sprite;
		}

		public virtual void DestroyMob()
		{
			ExitTile(pos);
			pos = null;
			nation?.mobsToRemove.Add(this);
		}


		public override void SerializeFields()
		{
			SerializeField(PosVector);
			SerializeField(sprite);
		}

		public override void DeserializeFields()
		{
			pos = tileMap?.getTile(DeserializeField<Vector2Int>());
			EnterTile(pos);
			sprite = DeserializeField<string>();
		}

	}
}
