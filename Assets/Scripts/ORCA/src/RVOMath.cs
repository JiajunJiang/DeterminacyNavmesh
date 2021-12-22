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
using DefaultNamespace;

namespace RVO
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    public struct RVOMath
    {
        /**
         * <summary>A sufficiently small positive number.</summary>
         */
        // internal const float RVO_EPSILON = 0.00001f;

        internal const int RVO_EPSILON = 0;
        
        /**
         * <summary>Computes the length of a specified two-dimensional vector.
         * </summary>
         *
         * <param name="point">The two-dimensional vector whose length is to be
         * computed.</param>
         * <returns>The length of the two-dimensional vector.</returns>
         */
        public static long abs(Point2D point)
        {
            return point.Magnitude;
        }

        /**
         * <summary>Computes the squared length of a specified two-dimensional
         * vector.</summary>
         *
         * <returns>The squared length of the two-dimensional vector.</returns>
         *
         * <param name="point">The two-dimensional vector whose squared length
         * is to be computed.</param>
         */
        public static long absSq(Point2D point)
        {
            return point.x * point.x + point.y * point.y;
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
        public static Point2D normalize(Point2D vector)
        {
            return vector / abs(vector);
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
        internal static long det(Point2D vector1, Point2D vector2)
        {
            return Point2D.Cross(vector1, vector2);
        }

        /**
         * <summary>Computes the squared distance from a line segment with the
         * specified endpoints to a specified point.</summary>
         *
         * <returns>The squared distance from the line segment to the point.
         * </returns>
         *
         * <param name="p1">The first endpoint of the line segment.</param>
         * <param name="p2">The second endpoint of the line segment.
         * </param>
         * <param name="p3">The point to which the squared distance is to
         * be calculated.</param>
         */
        internal static long distSqPointLineSegment(Point2D p1, Point2D p2, Point2D p3)
        {
            long r = ((p3 - p1) * (p2 - p1)).Magnitude / absSq(p2 - p1);

            if (r < 0.0f)
            {
                return absSq(p3 - p1);
            }

            if (r > 1.0f)
            {
                return absSq(p3 - p2);
            }

            return absSq(p3 - (p1 + (p2 - p1) * r));
        }

        internal static long longAbs(long scalar)
        {
            return Math.Abs(scalar);
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
        internal static long leftOf(Point2D a, Point2D b, Point2D c)
        {
            return det(a - c, b - a);
        }

        internal static long sqr(long scalar)
        {
            return scalar * scalar;
        }

        internal static long sqrt(long scalar)
        {
            return (long)Math.Sqrt(scalar);
        }
    }
}
