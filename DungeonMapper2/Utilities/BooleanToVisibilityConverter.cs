using System.Windows;

namespace DungeonMapper2.Utilities
{
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) {}
    }
}
