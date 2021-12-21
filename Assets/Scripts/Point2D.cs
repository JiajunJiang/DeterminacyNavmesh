using System;

namespace DefaultNamespace
{
    public struct Point2D : IEquatable<Point2D>
    {
        public long x;
        public long y;

        public long Magnitude => FixedMath.Sqrt(x * x + y * y);

        public Point2D(long x, long y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2D operator +(Point2D l, Point2D r)
        {
            return new Point2D(l.x + r.x, l.y + r.y);
        }

        public static Point2D operator -(Point2D l, Point2D r)
        {
            return new Point2D(l.x - r.x, l.y - r.y);
        }

        public static Point2D operator *(Point2D p, int m)
        {
            return new Point2D(p.x * m, p.y * m);
        }

        public static Point2D operator /(Point2D p, int m)
        {
            return new Point2D(p.x / m, p.y / m);
        }

        public static Point2D operator -(Point2D l)
        {
            return new Point2D(-l.x, -l.y);
        }

        public static bool operator ==(Point2D l, Point2D r)
        {
            long x = l.x - r.x;
            long y = l.y - r.y;
            return x * x + y * y == 0;
        }

        public static bool operator !=(Point2D l, Point2D r) => !(l == r);

        public static long Dot(Point2D l, Point2D r)
        {
            return l.x * r.x + l.y * r.y;
        }

        public static long Cross(Point2D l, Point2D r)
        {
            return l.x * r.y - r.x * l.y;
        }

        public static long Cross_XZ(Point3D l, Point3D r)
        {
            return l.x * r.z - r.x * l.z;
        }
        
        public static long Distance(Point2D l, Point2D r)
        {
            long num1 = l.x - r.x;
            long num2 = l.y - r.y;
            return FixedMath.Sqrt(num1 * num1 + num2 * num2);
        }

        public bool Equals(Point2D other)
        {
            return x == other.x && y == other.y;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        private static readonly Point2D ZeroVector = new Point2D(0, 0);
        private static readonly Point2D OneVector = new Point2D(1, 1);

        public static Point2D Zero => Point2D.ZeroVector;

        public static Point2D One => Point2D.OneVector;
    }
}