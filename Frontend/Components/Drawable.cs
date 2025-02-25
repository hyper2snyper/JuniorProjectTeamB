using JuniorProject.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JuniorProject.Frontend.Components
{
    public class Drawable
    {
        public string title;
        public Image image;
        public Boolean shouldDraw;
        public Boolean isMapOrGridlines;
        public Vector2Int pixelPosition;
        public Vector2Int gridPosition;
        public string path;
        public Drawable(Image image, Boolean shouldDraw, string title)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = true;
            this.title = title;

            if (title == "Grid")
            {
                shouldDraw = false;
                image.Opacity = 0.2;
            }
        }

        public Drawable(Image image, Boolean shouldDraw, string title, string path, Vector2Int pixelPosition, Vector2Int gridPosition)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = false;
            this.pixelPosition = pixelPosition;
            this.gridPosition = gridPosition;
            this.title = title;
            this.path = path;
        }

        public string getInformation()
        {
            String relativePath = path.Replace($"{Properties.Resources.ProjectDir}\\", "");
            return String.Format($"Grid Position -> [{gridPosition.X}, {gridPosition.Y}]\nImage Path -> {relativePath}\nisMapOrGridlines -> {isMapOrGridlines.ToString()}\nshouldDraw -> {shouldDraw.ToString()}");
        }
    }
}