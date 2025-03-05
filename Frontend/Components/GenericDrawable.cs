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
        public GenericDrawable(Vector2Int gridPosition, string sprite, int layer, string uniqueIdentifier = null) {
            this.gridPosition = gridPosition;
            this.sprite = sprite;
            this.layer = layer;
            this.uniqueIdentifier = uniqueIdentifier;
        }

        public Vector2Int gridPosition;
        public string sprite; 
        public int layer;
        public string uniqueIdentifier;
    }
}
