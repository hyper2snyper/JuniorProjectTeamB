using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JuniorProject.Frontend.Components
{
    public class Drawable
    {
        public Image image;
        public Boolean shouldDraw;
        public Boolean isMapOrGridlines;
        public int x, y;
        public Drawable(Image image, Boolean shouldDraw)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = true;
        }

        public Drawable(Image image, Boolean shouldDraw, int x, int y)
        {
            this.image = image;
            this.shouldDraw = shouldDraw;
            this.isMapOrGridlines = false;
            this.x = x;
            this.y = y;
        }
    }
}