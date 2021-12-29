using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FixedMath
{
    [Serializable]
    public struct JInt : IEquatable<JInt>, IComparable<JInt>, IComparable
    {
        public const int divscale = 1000;

        private const int div2scale = divscale * divscale;

        private long longvalue;

        public static JInt MaxValue = new JInt(int.MaxValue);

        public static JInt Zero = new JInt(0);

        public long IntValue
        {
            get { return longvalue; }
        }

        public float floatvalue
        {
            get { return (float) longvalue / divscale; }
        }

        public int IntSqr
        {
            get { return (int) (longvalue * longvalue / div2scale); }
        }

        public JInt IntSqrt
        {
            get
            {
                long value = JMath.isqrt(longvalue * divscale);
                return new JInt(value);
            }
        }

        public static JInt Max(JInt left, JInt right)
        {
            if (left > right)
            {
                return left;
            }

            return right;
        }

        public static JInt Min(JInt left, JInt right)
        {
            if (left > right)
            {
                return right;
            }

            return left;
        }

        private JInt(long value)
        {
            this.longvalue = value;
        }

        public override string ToString()
        {
            return string.Format("[{0}]", this.floatvalue);
        }

        public static JInt ToInt(int data)
        {
            JInt value = new JInt(data);
            return value;
        }

        public static JInt ToInt(long data)
        {
            JInt value = new JInt(data);
            return value;
        }

        public static implicit operator JInt(float data)
        {
            JInt newdata = new JInt();
            newdata.longvalue = Mathf.RoundToInt(data * divscale);
            return newdata;
        }

        #region add

        public static JInt operator +(JInt left, JInt right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue + right.longvalue;
            return value;
        }

        public static JInt operator +(JInt left, int right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static JInt operator +(JInt left, short right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static JInt operator +(JInt left, long right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static int operator +(int left, JInt right)
        {
            return (int) ((left * divscale + right.longvalue) / divscale);
        }

        public static short operator +(short left, JInt right)
        {
            return (short) ((left * divscale + right.longvalue) / divscale);
        }

        public static long operator +(long left, JInt right)
        {
            return (long) ((left * divscale + right.longvalue) / divscale);
        }

        #endregion

        #region reduce

        public static JInt operator -(JInt left, JInt right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue - right.longvalue;
            return value;
        }

        public static JInt operator -(JInt left, int right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static JInt operator -(JInt left, short right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static JInt operator -(JInt left, long right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static int operator -(int left, JInt right)
        {
            return (int) ((left * divscale - right.longvalue) / divscale);
        }

        public static short operator -(short left, JInt right)
        {
            return (short) ((left * divscale - right.longvalue) / divscale);
        }

        public static long operator -(long left, JInt right)
        {
            return (long) ((left * divscale - right.longvalue) / divscale);
        }

        #endregion

        #region multi

        public static JInt operator *(JInt left, JInt right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue * right.longvalue / divscale;
            return value;
        }

        public static JInt operator *(JInt left, int right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static JInt operator *(JInt left, short right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static JInt operator *(JInt left, long right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static int operator *(int left, JInt right)
        {
            return (int) ((left * right.longvalue) / divscale);
        }

        public static short operator *(short left, JInt right)
        {
            return (short) ((left * right.longvalue) / divscale);
        }

        public static long operator *(long left, JInt right)
        {
            return (int) ((left * right.longvalue) / divscale);
        }

        #endregion

        #region div

        public static JInt operator /(JInt left, JInt right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue * divscale / right.longvalue;
            return value;
        }

        public static JInt operator /(JInt left, short right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static JInt operator /(JInt left, int right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static JInt operator /(JInt left, long right)
        {
            JInt value = new JInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static short operator /(short left, JInt right)
        {
            return (short) (left * divscale / right.longvalue);
        }

        public static int operator /(int left, JInt right)
        {
            return (int) (left * divscale / right.longvalue);
        }

        public static long operator /(long left, JInt right)
        {
            return (long) (left * divscale / right.longvalue);
        }

        #endregion

        #region >

        public static bool operator >(JInt left, JInt right)
        {
            return left.longvalue > right.longvalue;
        }

        public static bool operator >(JInt left, short right)
        {
            return left.longvalue > right * divscale;
        }

        public static bool operator >(short left, JInt right)
        {
            return left * divscale > right;
        }

        public static bool operator >(int left, JInt right)
        {
            return left * divscale > right;
        }

        public static bool operator >(long left, JInt right)
        {
            return left * divscale > right;
        }

        public static bool operator >(JInt left, int right)
        {
            return left.longvalue > right * divscale;
        }

        public static bool operator >(JInt left, long right)
        {
            return left.longvalue > right * divscale;
        }

        #endregion

        #region <

        public static bool operator <(JInt left, JInt right)
        {
            return left.longvalue < right.longvalue;
        }

        public static bool operator <(JInt left, short right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(JInt left, int right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(JInt left, long right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(short left, JInt right)
        {
            return left * divscale < right.longvalue;
        }

        public static bool operator <(int left, JInt right)
        {
            return left * divscale < right.longvalue;
        }

        public static bool operator <(long left, JInt right)
        {
            return left * divscale < right.longvalue;
        }

        #endregion

        #region >=

        public static bool operator >=(JInt left, JInt right)
        {
            return left.longvalue >= right.longvalue;
        }

        public static bool operator >=(JInt left, int right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(JInt left, short right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(JInt left, long right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(short left, JInt right)
        {
            return left * divscale >= right.longvalue;
        }

        public static bool operator >=(int left, JInt right)
        {
            return left * divscale >= right.longvalue;
        }

        public static bool operator >=(long left, JInt right)
        {
            return left * divscale >= right.longvalue;
        }

        #endregion

        #region <=

        public static bool operator <=(JInt left, JInt right)
        {
            return left.longvalue <= right.longvalue;
        }

        public static bool operator <=(JInt left, int right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(JInt left, short right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(JInt left, long right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(short left, JInt right)
        {
            return left * divscale <= right.longvalue;
        }

        public static bool operator <=(int left, JInt right)
        {
            return left * divscale <= right.longvalue;
        }

        public static bool operator <=(long left, JInt right)
        {
            return left * divscale <= right.longvalue;
        }

        #endregion

        public static JInt operator -(JInt value)
        {
            JInt data = new JInt();
            data.longvalue = -value.longvalue;
            return data;
        }


        public static bool operator ==(JInt left, JInt right)
        {
            return left.longvalue == right.longvalue;
        }

        public static bool operator !=(JInt left, JInt right)
        {
            return left.longvalue != right.longvalue;
        }

        public bool Equals(JInt other)
        {
            return this.longvalue == other.longvalue;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(JInt value)
        {
            if (this == value)
            {
                return 0;
            }

            if (this > value)
            {
                return 1;
            }

            return -1;
        }

        public int CompareTo(object obj)
        {
            if (obj is JInt)
            {
                JInt value = (JInt) obj;
                if (this == value)
                {
                    return 0;
                }

                if (this > value)
                {
                    return 1;
                }
            }


            return -1;
        }
    }
}