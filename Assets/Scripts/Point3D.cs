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

    public long Magnitude => FixedMath.Sqrt(x * x + y * y + z * z);

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
}