using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;

namespace Capella
{
    public class TabImage : Image
    {
        public readonly double DefaultOpacity = 0.3;
        public readonly double SelectedOpacity = 0.8;

        public TabImage(){
            this.Opacity = 0.3;
        }
    }
}
