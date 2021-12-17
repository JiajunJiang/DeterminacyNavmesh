using System;

namespace DefaultNamespace
{
    public class Point2D : IEquatable<Point2D>
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

        public bool Equals(Point2D other)
        {
            return x == other.x && y == other.y;
        }
    }
}