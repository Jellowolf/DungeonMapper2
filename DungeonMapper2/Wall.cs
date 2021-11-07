using System;

namespace DungeonMapper2
{
    [Flags]
    public enum Wall
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
}
