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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
