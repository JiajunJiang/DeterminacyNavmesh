using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FixedMath
{
    public class JMath
    {
        private static Dictionary<long, long> sqrtcache = new Dictionary<long, long>();
        private const long sqrtmax = ((long) 1 << 62);

        public static long Abs(long value)
        {
            if (value < 0)
            {
                return -value;
            }

            return value;
        }

        public static int Max(int left, int right)
        {
            if (left > right)
            {
                return left;
            }

            return right;
        }

        public static int Min(int left, int right)
        {
            if (left > right)
            {
                return right;
            }

            return left;
        }

        public static long Max(long left, long right)
        {
            if (left > right)
            {
                return left;
            }

            return right;
        }

        public static long Min(long left, long right)
        {
            if (left > right)
            {
                return right;
            }

            return left;
        }

        public static int isqrt(long x)
        {
            long remainder = x > 0 ? x : -x;
            if (sqrtcache.ContainsKey(remainder))
            {
                long ret = sqrtcache[remainder];
                if (x < 0)
                {
                    ret = -ret;
                }

                return (int) ret;
            }

            long place = sqrtmax; //4 * 8 - 2

            while (place > remainder)
            {
                place /= 4;
            }

            long root = 0;
            while (place != 0)
            {
                if (remainder >= root + place)
                {
                    remainder -= root + place;
                    root += place * 2;
                }

                root /= 2;
                place /= 4;
            }

            sqrtcache[remainder] = root;

            if (x < 0)
            {
                root = -root;
            }

            return (int) root;
        }
    }
}