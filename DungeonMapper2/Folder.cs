namespace DungeonMapper2
{
    public class Folder : BasePathItem
    {
        public int? Id { get; set; }

        public Folder Parent { get; set; }
    }
}
