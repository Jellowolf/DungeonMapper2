using System.Collections.Generic;

namespace DungeonMapper2.Models
{
    public class BasePathItem : IPathItem
    {
        public string Name { get; set; }

        public List<IPathItem> ChildItems { get; set; }
    }
}
