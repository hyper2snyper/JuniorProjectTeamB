using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Frontend.Components
{
    public struct GenericDrawable
    {
        public enum DrawableType
        {
            Tile = 0,
            Building = 1,
            Unit = 2,
            Mob = 3,
            Debug = 4
        }

        public GenericDrawable(Vector2Int gridPosition, string sprite, DrawableType type, string uniqueIdentifier = null)
        {
            this.gridPosition = gridPosition;
            this.sprite = sprite;
            this.type = type;
            this.uniqueIdentifier = uniqueIdentifier;
        }

        public Vector2Int gridPosition;
        public string sprite;
        public int layer;
        public DrawableType type;
        public string uniqueIdentifier;
    }
}
