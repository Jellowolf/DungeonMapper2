namespace DungeonMapper2
{
    public class Tile
    {
        public int? Id { get; set; }

        public bool Traveled { get; set; }

        public Wall Walls { get; set; }

        public Wall Doors { get; set; }

        public Tile()
        {
            Traveled = false;
        }

        public Tile(bool traveled)
        {
            Traveled = traveled;
        }

        public void Clear()
        {
            Traveled = false;
            Walls = Wall.None;
            Doors = Wall.None;
        }
    }
}
