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
        public int x, y;
        public int xGrid, yGrid;
        public string path;
        public Drawable(Image image, Boolean shouldDraw, string title)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = true;
            this.title = title;
        }

        public Drawable(Image image, Boolean shouldDraw, string title, string path, int x, int y, int xGrid, int yGrid)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = false;
            this.x = x;
            this.y = y;
            this.xGrid = xGrid;
            this.yGrid = yGrid;
            this.title = title;
            this.path = path;
        }

        public string getInformation()
        {
            String relativePath = path.Replace($"{Properties.Resources.ProjectDir}\\", "");
            return String.Format($"Grid Position -> [{xGrid}, {yGrid}]\nImage Path -> {relativePath}");
        }
    }
}