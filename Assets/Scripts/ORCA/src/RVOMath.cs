/*
 * RVOMath.cs
 * RVO2 Library C#
 *
 * Copyright 2008 University of North Carolina at Chapel Hill
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

using System;
using FixedMath;

namespace RVO
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    public struct RVOMath
    {

        /**
         * <summary>Computes the absolute value of a float.</summary>
         *
         * <returns>The absolute value of the float.</returns>
         *
         * <param name="scalar">The float of which to compute the absolute
         * value.</param>
         */
        internal static JInt fabs(JInt scalar)
        {
            if(scalar < 0)
            {
                return -scalar;
            }

            return scalar;
        }
        /**
        * <summary>Computes the length of a specified two-dimensional vector.
        * </summary>
        *
        * <param name="vector">The two-dimensional vector whose length is to be
        * computed.</param>
        * <returns>The length of the two-dimensional vector.</returns>
        */
        public static JInt abs(Jint2 vector)
        {
            return vector.IntMagnitude ;
        }

        /**
        * <summary>Computes the normalization of the specified two-dimensional
        * vector.</summary>
        *
        * <returns>The normalization of the two-dimensional vector.</returns>
        *
        * <param name="vector">The two-dimensional vector whose normalization
        * is to be computed.</param>
        */
        public static Jint2 normalize(Jint2 vector)
        {
            return vector.normalized;
        }
        /**
         * <summary>Computes the squared length of a specified two-dimensional
         * vector.</summary>
         *
         * <returns>The squared length of the two-dimensional vector.</returns>
         *
         * <param name="vector">The two-dimensional vector whose squared length
         * is to be computed.</param>
         */
        public static JInt absSq(Jint2 vector)
        {
            return  JInt.ToInt((vector.IntX * vector.IntX + vector.IntY * vector.IntY)  * JInt.divscale / Jint2.div2scale);
        }

        public static JInt Dot(Jint2 left,Jint2 right)
        {
            return JInt.ToInt((left.IntX * right.IntX + left.IntY * right.IntY) * JInt.divscale / Jint2.div2scale);
        }

        /**
         * <summary>Computes the determinant of a two-dimensional square matrix
         * with rows consisting of the specified two-dimensional vectors.
         * </summary>
         *
         * <returns>The determinant of the two-dimensional square matrix.
         * </returns>
         *
         * <param name="vector1">The top row of the two-dimensional square
         * matrix.</param>
         * <param name="vector2">The bottom row of the two-dimensional square
         * matrix.</param>
         */
        internal static JInt det(Jint2 vector1, Jint2 vector2)
        {
            return JInt.ToInt((vector1.IntX * vector2.IntY - vector1.IntY * vector2.IntX) * JInt.divscale / Jint2.div2scale);
        }

        /**
         * <summary>Computes the squared distance from a line segment with the
         * specified endpoints to a specified point.</summary>
         *
         * <returns>The squared distance from the line segment to the point.
         * </returns>
         *
         * <param name="vector1">The first endpoint of the line segment.</param>
         * <param name="vector2">The second endpoint of the line segment.
         * </param>
         * <param name="vector3">The point to which the squared distance is to
         * be calculated.</param>
         */
        internal static JInt distSqPointLineSegment(Jint2 vector1, Jint2 vector2, Jint2 vector3)
        {
            JInt r = Dot(vector3 - vector1, vector2 - vector1) / absSq(vector2 - vector1);// (v31.IntX * v21.IntX  + v31.IntY * v21.IntY) * KInt.divscale / KInt2.div2scale;

            if (r < 0)
            {
                return absSq(vector3 - vector1);
            }

            if (r > 1)
            {
                return absSq(vector3 - vector2);
            }

            return absSq(vector3 - (vector1 + r * (vector2 - vector1)));
        }

        /**
         * <summary>Computes the signed distance from a line connecting the
         * specified points to a specified point.</summary>
         *
         * <returns>Positive when the point c lies to the left of the line ab.
         * </returns>
         *
         * <param name="a">The first point on the line.</param>
         * <param name="b">The second point on the line.</param>
         * <param name="c">The point to which the signed distance is to be
         * calculated.</param>
         */
        internal static JInt leftOf(Jint2 a, Jint2 b, Jint2 c)
        {
            return det(a - c, b - a);
        }


        internal static bool RightXZ(JInt3 a, JInt3 b, JInt3 p)
        {
            return (b.IntX - a.IntX) * (p.IntZ - a.IntZ) - (p.IntX - a.IntX) * (b.IntZ - a.IntZ) < 0;
        }


        internal static bool IsClockwiseXZ(JInt3 a, JInt3 b, JInt3 c)
        {
            return (b.IntX - a.IntX) * (c.IntZ - a.IntZ) - (c.IntX - a.IntX) * (b.IntZ - a.IntZ) < 0;
        }


        /**
         * <summary>Computes the square of a float.</summary>
         *
         * <returns>The square of the float.</returns>
         *
         * <param name="scalar">The float to be squared.</param>
         */
        internal static int sqr(int scalar)
        {
            return scalar * scalar;
        }

        internal static JInt sqr(JInt scalar)
        {
            return scalar.IntSqr;
        }

        internal static JInt sqrt(JInt scalar)
        {
            return scalar.IntSqrt;
        }
    }
}
