using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DungeonMapper2.Models
{
    public class BasePathItem : IPathItem
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int? Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<IPathItem> ChildItems { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        private bool _editModeEnabled;
        public bool EditModeEnabled
        {
            get => _editModeEnabled;
            set
            {
                _editModeEnabled = value;
                OnPropertyChanged(nameof(EditModeEnabled));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
