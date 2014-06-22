using System;

namespace ConwaysGameOfLife.Model
{
    public struct Cell : IEquatable<Cell>
    {
        public long X { get; set; }
        public long Y { get; set; }

        public override bool Equals(Object obj)
        {
            return obj is Cell && this == (Cell)obj;
        }

        public bool Equals(Cell obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Cell obj1, Cell obj2)
        {
            return obj1.X == obj2.X && obj1.Y == obj2.Y;
        }

        public static bool operator !=(Cell obj1, Cell obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
