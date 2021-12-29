using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public struct Point3D : IEquatable<Point3D>
{
    public long x;
    public long y;
    public long z;

    public long Magnitude => DefaultNamespace.FixedMath.Sqrt(x * x + y * y + z * z);

    public Point2D XZ => new Point2D(x, z);

    public Point3D(long x, long y, long z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Point3D(Vector3 vector3)
    {
        this.x = (int) vector3.x;
        this.y = (int) vector3.y;
        this.z = (int) vector3.z;
    }
    
    public Point3D(Point2D point2D)
    {
        this.x =  point2D.x;
        this.y =  0;
        this.z =  point2D.y;
    }

    public static Point3D operator +(Point3D l, Point3D r)
    {
        return new Point3D(l.x + r.x, l.y + r.y, l.z + r.z);
    }

    public static Point3D operator -(Point3D l, Point3D r)
    {
        return new Point3D(l.x - r.x, l.y - r.y, l.z - r.z);
    }

    public static Point3D operator *(Point3D p, int m)
    {
        return new Point3D(p.x * m, p.y * m, p.z * m);
    }

    public static Point3D operator *(Point3D p, long m)
    {
        return new Point3D(p.x * m, p.y * m, p.z * m);
    }

    public static Point3D operator /(Point3D p, int m)
    {
        return new Point3D(p.x / m, p.y / m, p.z / m);
    }

    public static Point3D operator /(Point3D p, long m)
    {
        return new Point3D(p.x / m, p.y / m, p.z / m);
    }

    public static Point3D operator -(Point3D l)
    {
        return new Point3D(-l.x, -l.y, -l.z);
    }

    public static bool operator ==(Point3D l, Point3D r)
    {
        long x = l.x - r.x;
        long y = l.y - r.y;
        long z = l.z - r.z;
        return x * x + y * y + z * z == 0;
    }

    public static bool operator !=(Point3D l, Point3D r) => !(l == r);

    public static long Dot(Point3D l, Point3D r)
    {
        return l.x * r.x + l.y * r.y + l.z * r.z;
    }

    public static Point3D Cross(Point3D l, Point3D r)
    {
        return new Point3D(
            l.y * r.z - l.z * r.y,
            l.z * r.x - l.x * r.z,
            l.x * r.y - l.y * r.x
        );
    }

    public static long Distance(Point3D l, Point3D r)
    {
        long num1 = l.x - r.x;
        long num2 = l.y - r.y;
        long num3 = l.z - r.z;
        return DefaultNamespace.FixedMath.Sqrt(num1 * num1 + num2 * num2 + num3 * num3);
    }

    public void Scale(Point3D scale)
    {
        this.x *= scale.x;
        this.y *= scale.y;
        this.z *= scale.z;
    }

    public Point3D Normalize(Point3D value)
    {
        return Magnitude > 0 ? value / Magnitude : Zero;
    }

    public long SqrMagnitude()
    {
        return this.x * this.x + this.y * this.y + this.z * this.z;
    }
    
    public override bool Equals(object other)
    {
        return other is Point3D && this.Equals(other);
    }

    public bool Equals(Point3D other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public Vector3 ToUnityVector3()
    {
        return new Vector3(x, y, z);
    }

    public override string ToString()
    {
        return $"({x},{y},{z})";
    }

    private static readonly Point3D ZeroVector = new Point3D(0, 0, 0);
    private static readonly Point3D OneVector = new Point3D(1, 1, 1);
    
    public static Point3D Zero => Point3D.ZeroVector;

    public static Point3D One => Point3D.OneVector;
}