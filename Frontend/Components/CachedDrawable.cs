using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using JuniorProject.Backend.Helpers;

namespace JuniorProject.Frontend.Components
{
    public class CachedDrawable
    {
        public Image image;

        public Vector2Int pixelPosition;
        public Vector2Int gridPosition;

        public Boolean shouldDelete;
        public Boolean shouldMove;

        public int layer;
        public string sprite;

        public CachedDrawable(Image image, Vector2Int pixelPos, Vector2Int gridPos, int layer, string sprite = "")
        {
            this.image = image;
            this.pixelPosition = pixelPos;
            this.gridPosition = gridPos;
            this.shouldDelete = false;
            this.shouldMove = false;
            this.sprite = sprite;
            this.layer = layer;
        }
    }
}
