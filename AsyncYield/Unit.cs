namespace AsyncYield
{
    public class Unit : System.IComparable
    {
        public int CompareTo(object obj) => 0;
        public override int GetHashCode() => 0;
        public override bool Equals(object obj)
        {
            if (obj == null) return true;
            if(obj.GetType() == typeof(Unit)) return true;
            return false;
        }
    }
}