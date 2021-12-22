// /*
//  * Vector2.cs
//  * RVO2 Library C#
//  *
//  * Copyright 2008 University of North Carolina at Chapel Hill
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  *
//  * Please send all bug reports to <geom@cs.unc.edu>.
//  *
//  * The authors may be contacted via:
//  *
//  * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
//  * Dept. of Computer Science
//  * 201 S. Columbia St.
//  * Frederick P. Brooks, Jr. Computer Science Bldg.
//  * Chapel Hill, N.C. 27599-3175
//  * United States of America
//  *
//  * <http://gamma.cs.unc.edu/RVO2/>
//  */
//
// using System;
// using System.Globalization;
//
// namespace RVO
// {
//     /**
//      * <summary>Defines a two-dimensional vector.</summary>
//      */
//     public struct Point2D
//     {
//         internal float x;
//         internal float y;
//
//         /**
//          * <summary>Constructs and initializes a two-dimensional vector from the
//          * specified xy-coordinates.</summary>
//          *
//          * <param name="x">The x-coordinate of the two-dimensional vector.
//          * </param>
//          * <param name="y">The y-coordinate of the two-dimensional vector.
//          * </param>
//          */
//         public Point2D(float x, float y)
//         {
//             this.x = x;
//             this.y = y;
//         }
//
//         /**
//          * <summary>Returns the string representation of this vector.</summary>
//          *
//          * <returns>The string representation of this vector.</returns>
//          */
//         public override string ToString()
//         {
//             return "(" + x.ToString(new CultureInfo("").NumberFormat) + "," + y.ToString(new CultureInfo("").NumberFormat) + ")";
//         }
//
//         /**
//          * <summary>Returns the x-coordinate of this two-dimensional vector.
//          * </summary>
//          *
//          * <returns>The x-coordinate of the two-dimensional vector.</returns>
//          */
//
//         /**
//          * <summary>Returns the y-coordinate of this two-dimensional vector.
//          * </summary>
//          *
//          * <returns>The y-coordinate of the two-dimensional vector.</returns>
//          */
//
//         /**
//          * <summary>Computes the dot product of the two specified
//          * two-dimensional vectors.</summary>
//          *
//          * <returns>The dot product of the two specified two-dimensional
//          * vectors.</returns>
//          *
//          * <param name="vector1">The first two-dimensional vector.</param>
//          * <param name="point2D">The second two-dimensional vector.</param>
//          */
//         public static float operator *(Point2D vector1, Point2D point2D)
//         {
//             return vector1.x * point2D.x + vector1.y * point2D.y;
//         }
//
//         /**
//          * <summary>Computes the scalar multiplication of the specified
//          * two-dimensional vector with the specified scalar value.</summary>
//          *
//          * <returns>The scalar multiplication of the specified two-dimensional
//          * vector with the specified scalar value.</returns>
//          *
//          * <param name="scalar">The scalar value.</param>
//          * <param name="vector">The two-dimensional vector.</param>
//          */
//         public static Point2D operator *(float scalar, Point2D vector)
//         {
//             return vector * scalar;
//         }
//
//         /**
//          * <summary>Computes the scalar multiplication of the specified
//          * two-dimensional vector with the specified scalar value.</summary>
//          *
//          * <returns>The scalar multiplication of the specified two-dimensional
//          * vector with the specified scalar value.</returns>
//          *
//          * <param name="vector">The two-dimensional vector.</param>
//          * <param name="scalar">The scalar value.</param>
//          */
//         public static Point2D operator *(Point2D vector, float scalar)
//         {
//             return new Point2D(vector.x * scalar, vector.y * scalar);
//         }
//
//         /**
//          * <summary>Computes the scalar division of the specified
//          * two-dimensional vector with the specified scalar value.</summary>
//          *
//          * <returns>The scalar division of the specified two-dimensional vector
//          * with the specified scalar value.</returns>
//          *
//          * <param name="vector">The two-dimensional vector.</param>
//          * <param name="scalar">The scalar value.</param>
//          */
//         public static Point2D operator /(Point2D vector, float scalar)
//         {
//             return new Point2D(vector.x / scalar, vector.y / scalar);
//         }
//
//         /**
//          * <summary>Computes the vector sum of the two specified two-dimensional
//          * vectors.</summary>
//          *
//          * <returns>The vector sum of the two specified two-dimensional vectors.
//          * </returns>
//          *
//          * <param name="vector1">The first two-dimensional vector.</param>
//          * <param name="point2D">The second two-dimensional vector.</param>
//          */
//         public static Point2D operator +(Point2D vector1, Point2D point2D)
//         {
//             return new Point2D(vector1.x + point2D.x, vector1.y + point2D.y);
//         }
//
//         /**
//          * <summary>Computes the vector difference of the two specified
//          * two-dimensional vectors</summary>
//          *
//          * <returns>The vector difference of the two specified two-dimensional
//          * vectors.</returns>
//          *
//          * <param name="vector1">The first two-dimensional vector.</param>
//          * <param name="point2D">The second two-dimensional vector.</param>
//          */
//         public static Point2D operator -(Point2D vector1, Point2D point2D)
//         {
//             return new Point2D(vector1.x - point2D.x, vector1.y - point2D.y);
//         }
//
//         /**
//          * <summary>Computes the negation of the specified two-dimensional
//          * vector.</summary>
//          *
//          * <returns>The negation of the specified two-dimensional vector.
//          * </returns>
//          *
//          * <param name="vector">The two-dimensional vector.</param>
//          */
//         public static Point2D operator -(Point2D vector)
//         {
//             return new Point2D(-vector.x, -vector.y);
//         }
//     }
// }
