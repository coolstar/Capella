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
