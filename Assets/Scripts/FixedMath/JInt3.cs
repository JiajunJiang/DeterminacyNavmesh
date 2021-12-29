using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FixedMath
{
    [Serializable]
    /// <summary>
    /// 剔除vector3操作中的隐式转换，提倡在kint系列中多用kint，而不是vector，减少vector的使用,减少数据差异的可能性 ，set, up 会自动进行转换，_x,Y,Z系列则直接覆盖
    /// </summary>
    public struct JInt3 : IEquatable<JInt3>
    {
        [SerializeField] private long _x;
        [SerializeField] private long _y;
        [SerializeField] private long _z;

        public const float scale = 1f / divscale;
        public const int divscale = 1000;

        public const int div2scale = divscale * divscale;

        public static readonly JInt3 zero = new JInt3(0, 0, 0);
        public static readonly JInt3 one = new JInt3(1, 1, 1);
        public static readonly JInt3 forward = new JInt3(0, 0, 1);
        public static readonly JInt3 up = new JInt3(0, 1, 0);
        public static readonly JInt3 right = new JInt3(1, 0, 0);

        public float this[int idx]
        {
            get
            {
                if (idx > 2)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    if (idx == 0)
                    {
                        return this.x;
                    }
                    else if (idx == 1)
                    {
                        return this.y;
                    }

                    return z;
                }
            }
            set
            {
                if (idx > 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    if (idx == 0)
                    {
                        this._x = (int) (value * divscale);
                    }
                    else if (idx == 1)
                    {
                        this._y = (int) (value * divscale);
                    }

                    this._z = (int) (value * divscale);
                }
            }
        }

        public float x
        {
            get { return _x * scale; }
        }

        public long IntX
        {
            get { return _x; }
            set { _x = value; }
        }

        public float y
        {
            get { return _y * scale; }
        }

        public long IntY
        {
            get { return _y; }
            set { _y = value; }
        }

        public float z
        {
            get { return _z * scale; }
        }

        public long IntZ
        {
            get { return _z; }
            set { _z = value; }
        }

        public float magnitude
        {
            get
            {
                float xx = _x * scale * _x * scale;
                float yy = _y * scale * _y * scale;
                float zz = _z * scale * _z * scale;
                return Mathf.Sqrt(xx + yy + zz);
            }
        }

        public int IntMagnitude
        {
            get
            {
                long xyz = _x * _x / div2scale + _y * _y / div2scale + _z * _z / div2scale;

                xyz = xyz < 0 ? -xyz : xyz;
                int v = JMath.isqrt(xyz);
#if UNITY_EDITOR
                float kv = Mathf.Sqrt(xyz);
                if (Math.Abs(kv - v) > 1)
                {
                    Debug.LogError("error");
                    v = (int) kv;
                }
#endif

                return v;
            }
        }

        public JInt3 normalized
        {
            get
            {
                int len = IntMagnitude;
                if (len == 0)
                {
                    return JInt3.zero;
                }

                JInt3 normlize = new JInt3();
                normlize._x = _x / len;
                normlize._y = _y / len;
                normlize._z = _z / len;

                return normlize;
            }
        }

        public long xzIntsqrMagnitude
        {
            get
            {
                long xx = _x * _x / (div2scale);
                long zz = _z * _z / (div2scale);
                return xx + zz;
            }
        }

        public float xzsqrMagnitude
        {
            get
            {
                float xx = _x * scale * _x * scale;
                float zz = _z * scale * _z * scale;
                return xx + zz;
            }
        }

        public float sqrMagnitude
        {
            get
            {
                float xx = _x * scale * _x * scale;
                float yy = _y * scale * _y * scale;
                float zz = _z * scale * _z * scale;
                return xx + yy + zz;
            }
        }

        public int IntsqrMagnitude
        {
            get
            {
                long xx = _x * _x / (div2scale);
                long yy = _y * _y / (div2scale);
                long zz = _z * _z / (div2scale);

                return (int) (xx + yy + zz);
            }
        }

        private JInt3(int kx, int ky, int kz)
        {
            this._x = kx * divscale;
            this._y = ky * divscale;
            this._z = kz * divscale;
        }

        public JInt3(float kx, float ky, float kz)
        {
            this._x = (int) (kx * divscale);
            this._y = (int) (ky * divscale);
            this._z = (int) (kz * divscale);
        }

        public JInt3(Vector3 pos)
        {
            this._x = (int) (pos.x * divscale);
            this._y = (int) (pos.y * divscale);
            this._z = (int) (pos.z * divscale);
        }


        public static JInt3 ToInt3(int x, int y, int z)
        {
            JInt3 value = new JInt3();
            value._x = x;
            value._y = y;
            value._z = z;
            return value;
        }


        public static JInt3 ToInt3(long x, long y, long z)
        {
            JInt3 value = new JInt3();
            value._x = x;
            value._y = y;
            value._z = z;
            return value;
        }

        public static JInt3 ToInt3(JInt x, JInt y, JInt z)
        {
            JInt3 value = new JInt3();
            value._x = x.IntValue / JInt.divscale * divscale;
            value._y = y.IntValue / JInt.divscale * divscale;
            value._z = z.IntValue / JInt.divscale * divscale;
            return value;
        }

        public void Copy(int px, int py, int pz)
        {
            this._x = px;
            this._y = py;
            this._z = pz;
        }

        public void UpdateX(int dx)
        {
            this._x += dx * divscale;
        }

        public void UpdateX(short dx)
        {
            this._x += dx * divscale;
        }

        public void UpdateX(long dx)
        {
            this._x += (int) (dx * divscale);
        }

        public void UpdateX(float dx)
        {
            this._x += (int) (dx * divscale);
        }

        public void UpdateY(int dy)
        {
            this._y += dy * divscale;
        }

        public void UpdateY(short dy)
        {
            this._y += dy * divscale;
        }

        public void UpdateY(long dy)
        {
            this._y += (int) (dy * divscale);
        }

        public void UpdateY(float dy)
        {
            this._y += (int) (dy * divscale);
        }


        public void UpdateZ(int dz)
        {
            this._z += dz * divscale;
        }

        public void UpdateZ(short dz)
        {
            this._z += dz * divscale;
        }

        public void UpdateZ(long dz)
        {
            this._z += (int) (dz * divscale);
        }

        public void UpdateZ(float dz)
        {
            this._z += (int) (dz * divscale);
        }

        /// set
        /// 
        public void SetX(int dx)
        {
            this._x = dx * divscale;
        }

        public void SetX(short dx)
        {
            this._x = dx * divscale;
        }

        public void SetX(long dx)
        {
            this._x = (int) (dx * divscale);
        }

        public void SetX(float dx)
        {
            this._x = (int) (dx * divscale);
        }

        public void SetY(int dy)
        {
            this._y = dy * divscale;
        }

        public void SetY(short dy)
        {
            this._y = dy * divscale;
        }

        public void SetY(long dy)
        {
            this._y = (int) (dy * divscale);
        }

        public void SetY(float dy)
        {
            this._y = (int) (dy * divscale);
        }

        public void SetZ(int dz)
        {
            this._z = dz * divscale;
        }

        public void SetZ(short dz)
        {
            this._z = dz * divscale;
        }

        public void SetZ(long dz)
        {
            this._z = (int) (dz * divscale);
        }

        public void SetZ(float dz)
        {
            this._z = (int) (dz * divscale);
        }

        public static JInt3 Lerp(JInt3 a, JInt3 b, float f)
        {
            JInt3 data = new JInt3();
            data._x = (long) (a._x * (1f - f) + (b._x * f));
            data._y = (long) (a._y * (1f - f) + (b._y * f));
            data._z = (long) (a._z * (1f - f) + (b._z * f));

            return data;
        }

        public static JInt3 Lerp(JInt3 a, JInt3 b, int percent, int max)
        {
            JInt3 data = new JInt3();
            data._x = (a._x * (max - percent) / max + (b._x * percent) / max);
            data._y = (a._y * (max - percent) / max + (b._y * percent) / max);
            data._z = (a._z * (max - percent) / max + (b._z * percent) / max);

            return data;
        }

        public static float Dot(JInt3 left, JInt3 right)
        {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2}]", this._x * scale, this._y * scale, this._z * scale);
        }

        long getvalue(long idx)
        {
            if (idx == 0)
            {
                return _x;
            }
            else if (idx == 1)
            {
                return _y;
            }
            else if (idx == 2)
            {
                return _z;
            }

            throw new ArgumentException(idx.ToString());
        }

        const long v1 = (long) 1 << 20;

        long SimpleHash()
        {
            long hash = 17;

            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + (_x) % v1;
            hash = hash * 23 + (_y) % v1;
            hash = hash * 23 + (_z) % v1;
            return hash;
        }

        public override int GetHashCode()
        {
            return (int) SimpleHash();
        }

        public bool Equals(JInt3 other)
        {
            if (this._x != other._x) return false;
            if (this._y != other._y) return false;
            if (this._z != other._z) return false;

            return true;
        }

        public override bool Equals(System.Object obj)
        {
            return base.Equals(obj);
        }

        public static implicit operator Vector3(JInt3 data)
        {
            return new Vector3(data._x * scale, data._y * scale, data._z * scale);
        }

        public static explicit operator JInt3(Vector3 data)
        {
            return new JInt3(data);
        }

        public static JInt3 operator -(JInt3 vector)
        {
            JInt3 ki3 = new JInt3();
            ki3._x = -vector._x;
            ki3._y = -vector._y;
            ki3._z = -vector._z;
            return ki3;
        }

        #region add

        public static JInt3 operator +(JInt3 left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = left._x + right._x;
            value._y = left._y + right._y;
            value._z = left._z + right._z;
            return value;
        }

        public static JInt3 operator +(JInt3 left, Vector3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left._x + right.x * divscale);
            value._y = (long) (left._y + right.y * divscale);
            value._z = (long) (left._z + right.z * divscale);
            return value;
        }

        public static JInt3 operator +(Vector3 right, JInt3 left)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left._x + right.x * divscale);
            value._y = (long) (left._y + right.y * divscale);
            value._z = (long) (left._z + right.z * divscale);
            return value;
        }

        public static JInt3 operator +(JInt3 left, int right)
        {
            JInt3 value = new JInt3();
            value._x = left._x + right * divscale;
            value._y = left._y + right * divscale;
            value._z = left._z + right * divscale;
            return value;
        }

        public static JInt3 operator +(JInt3 left, short right)
        {
            JInt3 value = new JInt3();
            value._x = left._x + right * divscale;
            value._y = left._y + right * divscale;
            value._z = left._z + right * divscale;
            return value;
        }

        public static JInt3 operator +(JInt3 left, float right)
        {
            int r = (int) (right * divscale);
            JInt3 value = new JInt3();
            value._x = left._x + r;
            value._y = left._y + r;
            value._z = left._z + r;
            return value;
        }

        public static JInt3 operator +(JInt3 left, JInt right)
        {
            int r = (int) (right.IntValue / JInt.divscale * divscale);
            JInt3 value = new JInt3();
            value._x = left._x + r;
            value._y = left._y + r;
            value._z = left._z + r;
            return value;
        }

        public static JInt3 operator +(int left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x + left * divscale;
            value._y = right._y + left * divscale;
            value._z = right._z + left * divscale;
            return value;
        }

        public static JInt3 operator +(short left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x + left * divscale;
            value._y = right._y + left * divscale;
            value._z = right._z + left * divscale;
            return value;
        }

        public static JInt3 operator +(float left, JInt3 right)
        {
            int r = (int) (left * divscale);
            JInt3 value = new JInt3();
            value._x = right._x + r;
            value._y = right._y + r;
            value._z = right._z + r;
            return value;
        }

        public static JInt3 operator +(JInt left, JInt3 right)
        {
            int r = (int) (left.IntValue / JInt.divscale * divscale);
            JInt3 value = new JInt3();
            value._x = right._x + r;
            value._y = right._y + r;
            value._z = right._z + r;
            return value;
        }

        #endregion

        #region reduce

        public static JInt3 operator -(JInt3 left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = left._x - right._x;
            value._y = left._y - right._y;
            value._z = left._z - right._z;
            return value;
        }

        public static JInt3 operator -(JInt3 left, Vector3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left._x - right.x * divscale);
            value._y = (long) (left._y - right.y * divscale);
            value._z = (long) (left._z - right.z * divscale);
            return value;
        }

        public static JInt3 operator -(Vector3 left, JInt3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left.x * divscale - right._x);
            value._y = (long) (left.y * divscale - right._y);
            value._z = (long) (left.z * divscale - right._z);
            return value;
        }

        public static JInt3 operator -(JInt3 left, int right)
        {
            int r = (int) (right * divscale);
            JInt3 value = new JInt3();
            value._x = left._x - r;
            value._y = left._y - r;
            value._z = left._z - r;
            return value;
        }

        public static JInt3 operator -(JInt3 left, short right)
        {
            int r = (int) (right * divscale);
            JInt3 value = new JInt3();
            value._x = left._x - r;
            value._y = left._y - r;
            value._z = left._z - r;
            return value;
        }

        public static JInt3 operator -(JInt3 left, float right)
        {
            int r = (int) (right * divscale);
            JInt3 value = new JInt3();
            value._x = left._x - r;
            value._y = left._y - r;
            value._z = left._z - r;
            return value;
        }

        public static JInt3 operator -(JInt3 left, JInt right)
        {
            int r = (int) (right.IntValue / JInt.divscale * divscale);
            JInt3 value = new JInt3();
            value._x = left._x - r;
            value._y = left._y - r;
            value._z = left._z - r;
            return value;
        }

        public static JInt3 operator -(int left, JInt3 right)
        {
            int r = (int) (left * divscale);
            JInt3 value = new JInt3();
            value._x = right._x - r;
            value._y = right._y - r;
            value._z = right._z - r;
            return value;
        }

        public static JInt3 operator -(short left, JInt3 right)
        {
            int r = (int) (left * divscale);
            JInt3 value = new JInt3();
            value._x = right._x - r;
            value._y = right._y - r;
            value._z = right._z - r;
            return value;
        }

        public static JInt3 operator -(float left, JInt3 right)
        {
            int r = (int) (left * divscale);
            JInt3 value = new JInt3();
            value._x = right._x - r;
            value._y = right._y - r;
            value._z = right._z - r;
            return value;
        }

        public static JInt3 operator -(JInt left, JInt3 right)
        {
            int r = (int) (left.IntValue / JInt.divscale * divscale);
            JInt3 value = new JInt3();
            value._x = right._x - r;
            value._y = right._y - r;
            value._z = right._z - r;
            return value;
        }

        #endregion

        #region multi

        public static JInt3 operator *(JInt3 left, JInt3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = left._x * right._x / divscale;
            value._y = left._y * right._y / divscale;
            value._z = left._z * right._z / divscale;
            return value;
        }

        public static JInt3 operator *(JInt3 left, Vector3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left._x * right.x);
            value._y = (long) (left._y * right.y);
            value._z = (long) (left._z * right.z);
            return value;
        }

        public static JInt3 operator *(Vector3 left, JInt3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left.x * right._x);
            value._y = (long) (left.y * right._y);
            value._z = (long) (left.z * right._z);
            return value;
        }

        public static JInt3 operator *(JInt3 left, int right)
        {
            JInt3 value = new JInt3();
            value._x = left._x * right;
            value._y = left._y * right;
            value._z = left._z * right;
            return value;
        }

        public static JInt3 operator *(JInt3 left, short right)
        {
            JInt3 value = new JInt3();
            value._x = left._x * right;
            value._y = left._y * right;
            value._z = left._z * right;
            return value;
        }

        public static JInt3 operator *(JInt3 left, float right)
        {
            JInt3 value = new JInt3();
            value._x = (long) (left._x * right);
            value._y = (long) (left._y * right);
            value._z = (long) (left._z * right);
            return value;
        }

        public static JInt3 operator *(JInt3 left, JInt right)
        {
            JInt3 value = new JInt3();
            value._x = (left._x * right.IntValue / JInt.divscale);
            value._y = (left._y * right.IntValue / JInt.divscale);
            value._z = (left._z * right.IntValue / JInt.divscale);
            return value;
        }

        public static JInt3 operator *(int left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x * left;
            value._y = right._y * left;
            value._z = right._z * left;
            return value;
        }

        public static JInt3 operator *(short left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x * left;
            value._y = right._y * left;
            value._z = right._z * left;
            return value;
        }

        public static JInt3 operator *(float left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = (long) (right._x * left);
            value._y = (long) (right._y * left);
            value._z = (long) (right._z * left);
            return value;
        }

        public static JInt3 operator *(JInt left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = (long) (right._x * left.IntValue / JInt.divscale);
            value._y = (long) (right._y * left.IntValue / JInt.divscale);
            value._z = (long) (right._z * left.IntValue / JInt.divscale);
            return value;
        }

        #endregion

        #region div

        private static long Div(long left, long right)
        {
            if (left == 0)
            {
                return 0;
            }
            else if (right == 0)
            {
                Debug.LogError("DivideByZeroException: Division by zero");
                return long.MaxValue;
            }

            return left / right;
        }

        public static JInt3 operator /(JInt3 left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = Div(left._x * divscale, right._x);
            value._y = Div(left._y * divscale, right._y);
            value._z = Div(left._z * divscale, right._z);
            return value;
        }

        public static JInt3 operator /(JInt3 left, Vector3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left._x / right.x);
            value._y = (long) (left._y / right.y);
            value._z = (long) (left._z / right.z);
            return value;
        }

        public static JInt3 operator /(Vector3 left, JInt3 right)
        {
            //keep one scale
            JInt3 value = new JInt3();
            value._x = (long) (left.x * divscale / right._x);
            value._y = (long) (left.y * divscale / right._y);
            value._z = (long) (left.z * divscale / right._z);
            return value;
        }

        public static JInt3 operator /(JInt3 left, int right)
        {
            JInt3 value = new JInt3();
            value._x = left._x / right;
            value._y = left._y / right;
            value._z = left._z / right;
            return value;
        }

        public static JInt3 operator /(JInt3 left, short right)
        {
            JInt3 value = new JInt3();
            value._x = left._x / right;
            value._y = left._y / right;
            value._z = left._z / right;
            return value;
        }

        public static JInt3 operator /(JInt3 left, float right)
        {
            JInt3 value = new JInt3();
            value._x = (long) (left._x / right);
            value._y = (long) (left._y / right);
            value._z = (long) (left._z / right);
            return value;
        }

        public static JInt3 operator /(JInt3 left, JInt right)
        {
            JInt3 value = new JInt3();
            value._x = (left._x * JInt.divscale / right.IntValue);
            value._y = (left._y * JInt.divscale / right.IntValue);
            value._z = (left._z * JInt.divscale / right.IntValue);
            return value;
        }

        public static JInt3 operator /(int left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x / left;
            value._y = right._y / left;
            value._z = right._z / left;
            return value;
        }

        public static JInt3 operator /(short left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = right._x / left;
            value._y = right._y / left;
            value._z = right._z / left;
            return value;
        }

        public static JInt3 operator /(float left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = (long) (right._x / left);
            value._y = (long) (right._y / left);
            value._z = (long) (right._z / left);
            return value;
        }

        public static JInt3 operator /(JInt left, JInt3 right)
        {
            JInt3 value = new JInt3();
            value._x = (right._x * JInt.divscale / left.IntValue);
            value._y = (right._y * JInt.divscale / left.IntValue);
            value._z = (right._z * JInt.divscale / left.IntValue);
            return value;
        }

        #endregion

        public static bool operator ==(JInt3 left, JInt3 right)
        {
            return left._x == right._x && left._y == right._y && left._z == right._z;
        }

        public static bool operator !=(JInt3 left, JInt3 right)
        {
            return !(left._x == right._x && left._y == right._y && left._z == right._z);
        }
    }
}