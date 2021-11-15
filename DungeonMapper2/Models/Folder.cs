namespace DungeonMapper2.Models
{
    public class Folder : BasePathItem
    {
        public Folder Parent { get; set; }

        public override SegoeIcon Icon => IsExpanded ? SegoeIcon.TreeFolderFolderOpen : SegoeIcon.TreeFolderFolder;
    }
}
