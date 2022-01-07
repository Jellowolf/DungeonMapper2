using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DungeonMapperStandard.Models
{
    public interface IPathItem : INotifyPropertyChanged
    {
        int? Id { get; set; }

        string Name { get; set; }

        ObservableCollection<IPathItem> ChildItems { get; set; }

        bool IsSelected { get; set; }

        bool IsExpanded { get; set; }

        bool EditModeEnabled { get; set; }

        SegoeIcon Icon { get; }
    }
}
