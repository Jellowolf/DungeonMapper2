using System.Windows;
using System.Windows.Media;

namespace DungeonMapper2
{
    public class TileHost : FrameworkElement
    {
        public Visual VisualElement { get; set; }

        protected override int VisualChildrenCount
        {
            get { return VisualElement != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return VisualElement;
        }
    }
}
