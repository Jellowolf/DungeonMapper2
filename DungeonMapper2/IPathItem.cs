using System.Collections.Generic;

namespace DungeonMapper2
{
    public interface IPathItem
    {
        string Name { get; set; }

        List<IPathItem> ChildItems { get; set; }
    }
}
