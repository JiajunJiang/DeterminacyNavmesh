namespace DefaultNamespace
{
    public class LineSegment
    {
        public int p1;
        public int p2;

        public LineSegment(int a, int b)
        {
            p1 = a; p2 = b;
        }
        public override bool Equals(object other) =>
            (p1 == ((LineSegment)other).p1 && p2 == ((LineSegment)other).p2) ||
            (p1 == ((LineSegment)other).p2 && p2 == ((LineSegment)other).p1);

        public override int GetHashCode()
        {
            return p1 * p2;
        }
    }
}