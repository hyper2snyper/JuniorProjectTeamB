﻿using JuniorProject.Backend.Helpers;
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
		public int layer = 0;

		public Mob() { }

		public Mob(TileMap tileMap, TileMap.Tile pos, Nation? nation)
		{
			this.nation = nation;
			this.tileMap = tileMap;
			EnterTile(pos);
		}

		public virtual void TakeTurn(ulong tick) { }


		public virtual bool TryEnter(TileMap.Tile tile) { return true; }
		
		public virtual void EnterTile(TileMap.Tile tile) 
		{
			ExitTile(pos);
			pos = tile;
			tileMap.TilesUpdated();
		}

		public virtual void ExitTile(TileMap.Tile tile) { }

		public void populateDrawables(ref List<GenericDrawable> genericDrawables)
		{
			genericDrawables.Add(new GenericDrawable(PosVector, GetSprite(), layer));
		}

		public virtual string GetSprite()
		{
			return sprite;
		}

		public override void SerializeFields()
		{
			SerializeField(PosVector);
			SerializeField(sprite);
		}

		public override void DeserializeFields()
		{
			pos = tileMap?.getTile(DeserializeField<Vector2Int>());
			sprite = DeserializeField<string>();
		}

	}
}
