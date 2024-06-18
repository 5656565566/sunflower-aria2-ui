using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sunflower_aria2_ui
{
    public class Config
    {


        public static Color UIColor = Color.FromArgb(35, 40, 35);

        public static Color InvertColor()
        {
            return Color.FromArgb(UIColor.A, 255 - UIColor.R, 255 - UIColor.G, 255 - UIColor.B);
        }
    }
}
