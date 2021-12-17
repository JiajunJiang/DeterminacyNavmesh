using System;
using System.Runtime.CompilerServices;

namespace DefaultNamespace
{
    public class FixedMath
    {
        public static long Sqrt(long n)
        {
            if (n <= 0) return 0;

            long x = 2 << Cob(n) / 2;
            //int x = 15;
            //while (-~x * -~x < n || ~-x * ~-x > n)
            while (x * x > n)
            {
                x = (x + n / x) >> 1;
            }

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int Cob(float v)
        {
            return -~(int) (*(uint*) &v << 2 >> 25);
        }

        public static bool IsLineCross(Point2D x1, Point2D y1, Point2D x2, Point2D y2)
        {
            return Math.Sign(Point2D.Cross(x1 - x2, y1 - x2)) * Math.Sign(Point2D.Cross(x1 - y2, y1 - y2)) < 0 &&
                   Math.Sign(Point2D.Cross(x2 - x1, y2 - x1)) * Math.Sign(Point2D.Cross(x2 - y1, y2 - y1)) < 0;
        }

        public static bool IsInTriangle(Point2D a, Point2D b, Point2D c, Point2D p)
        {
            var pa = a - p;
            var pb = b - p;
            var pc = c - p;

            if ((pa.x > 0 && pb.x > 0 && pc.x > 0) ||
                (pa.x < 0 && pb.x < 0 && pc.x < 0) ||
                (pa.y > 0 && pb.y > 0 && pc.y > 0) ||
                (pa.y < 0 && pb.y < 0 && pc.y < 0))
                return false;

            var crossAB = Math.Sign(Point2D.Cross(pa, pb));
            var crossBC = Math.Sign(Point2D.Cross(pb, pc));
            var crossCA = Math.Sign(Point2D.Cross(pc, pa));

            return crossAB * crossBC >= 0 && crossBC * crossCA >= 0;
        }

        public static bool IsInTriangleXZ(Point3D a, Point3D b, Point3D c, Point3D p)
        {
            return IsInTriangle(a.XZ, b.XZ, c.XZ, p.XZ);
        }
    }
}