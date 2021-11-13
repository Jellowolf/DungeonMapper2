using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace DungeonMapper2.Utilities
{
    public class SetFocusAction : TargetedTriggerAction<UIElement>
    {
        protected override void Invoke(object parameter)
        {
            Target?.Focus();
        }
    }
}
