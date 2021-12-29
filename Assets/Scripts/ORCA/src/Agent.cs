/*
 * Agent.cs
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
using System.Collections.Generic;
using FixedMath;
using UnityEngine;

namespace RVO
{
    /**
     * <summary>Defines an agent in the simulation.</summary>
     */
    internal class Agent
    {
        internal IList<KeyValuePair<JInt, Agent>> agentNeighbors_ = new List<KeyValuePair<JInt, Agent>>();
        internal IList<KeyValuePair<JInt, Obstacle>> obstacleNeighbors_ = new List<KeyValuePair<JInt, Obstacle>>();
        internal IList<Line> orcaLines_ = new List<Line>();
        internal Jint2 position_;
        internal Jint2 prefVelocity_;
        internal Jint2 velocity_;
        internal int id_ = 0;
        internal int maxNeighbors_ = 0;
        internal JInt maxSpeed_ = 0;
        internal JInt neighborDist_ = 0;
        internal JInt radius_ = 0;
        internal JInt timeHorizon_ = 0;
        internal JInt timeHorizonObst_ = 0;
        internal bool needDelete_ = false;

        private Jint2 newVelocity_;

        /**
         * <summary>Computes the neighbors of this agent.</summary>
         */
        internal void computeNeighbors()
        {
            obstacleNeighbors_.Clear();
            JInt rangeSq = RVOMath.sqr(timeHorizonObst_ * maxSpeed_ + radius_);
            Simulator.Instance.kdTree_.computeObstacleNeighbors(this, rangeSq);
            agentNeighbors_.Clear();

            if (maxNeighbors_ > 0)
            {
                rangeSq = RVOMath.sqr(neighborDist_);
                Simulator.Instance.kdTree_.computeAgentNeighbors(this, ref rangeSq);
            }
        }
        bool isover(Jint2 dir)
        {
            return dir.x > 100 || dir.y > 100;
        }

        /**
         * <summary>Computes the new velocity of this agent.</summary>
         */
        internal void computeNewVelocity()
        {
            orcaLines_.Clear();

            //KInt invTimeHorizonObst = 1 / timeHorizonObst_;

            JInt tempradius = radius_ / timeHorizonObst_;
            /* Create obstacle ORCA lines. */
            for (int i = 0; i < obstacleNeighbors_.Count; ++i)
            {

                Obstacle obstacle1 = obstacleNeighbors_[i].Value;
                Obstacle obstacle2 = obstacle1.next_;

                Jint2 relativePosition1 = obstacle1.point_ - position_;
                Jint2 relativePosition2 = obstacle2.point_ - position_;

                /*
                 * Check if velocity obstacle of obstacle is already taken care
                 * of by previously constructed obstacle ORCA lines.
                 */
                bool alreadyCovered = false;

                for (int j = 0; j < orcaLines_.Count; ++j)
                {
                    if (RVOMath.det( relativePosition1 / timeHorizonObst_ - orcaLines_[j].point, orcaLines_[j].direction) - tempradius >= 0 && RVOMath.det(relativePosition2 / timeHorizonObst_ - orcaLines_[j].point, orcaLines_[j].direction) - tempradius >= 0)
                    {
                        alreadyCovered = true;

                        break;
                    }
                }

                if (alreadyCovered)
                {
                    continue;
                }

                /* Not yet covered. Check for collisions. */
                JInt distSq1 = RVOMath.absSq(relativePosition1);
                JInt distSq2 = RVOMath.absSq(relativePosition2);

                JInt radiusSq = RVOMath.sqr(radius_);

                Jint2 obstacleVector = obstacle2.point_ - obstacle1.point_;
                JInt s = (-RVOMath.Dot( relativePosition1 , obstacleVector)) / RVOMath.absSq(obstacleVector);
                JInt distSqLine = RVOMath.absSq(-relativePosition1 - s * obstacleVector);

                Line line = new Line();

                if (s < 0 && distSq1 <= radiusSq)
                {
                    /* Collision with left vertex. Ignore if non-convex. */
                    if (obstacle1.convex_)
                    {
                        line.point = Jint2.zero;
                        line.direction = RVOMath.normalize( Jint2.ToInt2(-relativePosition1.IntY, relativePosition1.IntX));
                        orcaLines_.Add(line);
                    }

                    continue;
                }
                else if (s > 1 && distSq2 <= radiusSq)
                {
                    /*
                     * Collision with right vertex. Ignore if non-convex or if
                     * it will be taken care of by neighboring obstacle.
                     */
                    if (obstacle2.convex_ && RVOMath.det(relativePosition2, obstacle2.direction_) >= 0)
                    {
                        line.point = Jint2.zero;
                        line.direction = RVOMath.normalize(Jint2.ToInt2(-relativePosition2.IntY, relativePosition2.IntX));
                        orcaLines_.Add(line);
                    }

                    continue;
                }
                else if (s >= 0 && s < 1 && distSqLine <= radiusSq)
                {
                    /* Collision with obstacle segment. */
                    line.point = Jint2.zero;
                    line.direction = -obstacle1.direction_;
                    orcaLines_.Add(line);

                    continue;
                }

                /*
                 * No collision. Compute legs. When obliquely viewed, both legs
                 * can come from a single vertex. Legs extend cut-off line when
                 * non-convex vertex.
                 */

                Jint2 leftLegDirection, rightLegDirection;

                if (s < 0 && distSqLine <= radiusSq)
                {
                    /*
                     * Obstacle viewed obliquely so that left vertex
                     * defines velocity obstacle.
                     */
                    if (!obstacle1.convex_)
                    {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle2 = obstacle1;

                    JInt leg1 = RVOMath.sqrt(distSq1 - radiusSq);
                    leftLegDirection = Jint2.ToInt2(relativePosition1.IntX * leg1 - relativePosition1.IntY * radius_, relativePosition1.IntX * radius_ + relativePosition1.IntY * leg1) / distSq1;
                    rightLegDirection = Jint2.ToInt2(relativePosition1.IntX * leg1 + relativePosition1.IntY * radius_, -relativePosition1.IntX * radius_ + relativePosition1.IntY * leg1) / distSq1;
                    if(isover(leftLegDirection) || isover(rightLegDirection))
                    {
                        Debug.LogError("!!!");
                    }
                }
                else if (s > 1 && distSqLine <= radiusSq)
                {
                    /*
                     * Obstacle viewed obliquely so that
                     * right vertex defines velocity obstacle.
                     */
                    if (!obstacle2.convex_)
                    {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle1 = obstacle2;

                    JInt leg2 = RVOMath.sqrt(distSq2 - radiusSq);
                    leftLegDirection = Jint2.ToInt2(relativePosition2.IntX * leg2 - relativePosition2.IntY * radius_, relativePosition2.IntX * radius_ + relativePosition2.IntY * leg2) / distSq2;
                    rightLegDirection = Jint2.ToInt2(relativePosition2.IntX * leg2 + relativePosition2.IntY * radius_, -relativePosition2.IntX * radius_ + relativePosition2.IntY * leg2) / distSq2;
                    if (isover(leftLegDirection) || isover(rightLegDirection))
                    {
                        Debug.LogError("!!!");
                    }
                }
                else
                {
                    /* Usual situation. */
                    if (obstacle1.convex_)
                    {
                        JInt leg1 = RVOMath.sqrt(distSq1 - radiusSq);
                        leftLegDirection = Jint2.ToInt2(relativePosition1.IntX * leg1 - relativePosition1.IntY * radius_, relativePosition1.IntX * radius_ + relativePosition1.IntY * leg1) / distSq1;
                        if (isover(leftLegDirection) )
                        {
                            Debug.LogError("!!!");
                        }
                    }
                    else
                    {
                        /* Left vertex non-convex; left leg extends cut-off line. */
                        leftLegDirection = -obstacle1.direction_;
                        if (isover(leftLegDirection) )
                        {
                            Debug.LogError("!!!");
                        }
                    }

                    if (obstacle2.convex_)
                    {
                        JInt leg2 = RVOMath.sqrt(distSq2 - radiusSq);

                        rightLegDirection = Jint2.ToInt2(relativePosition2.IntX * leg2 + relativePosition2.IntY * radius_, -relativePosition2.IntX * radius_ + relativePosition2.IntY * leg2) / distSq2;
                        if ( isover(rightLegDirection))
                        {
                            Debug.LogError("!!!");
                        }
                    }
                    else
                    {
                        /* Right vertex non-convex; right leg extends cut-off line. */
                        rightLegDirection = obstacle1.direction_;
                        if ( isover(rightLegDirection))
                        {
                            Debug.LogError("!!!");
                        }
                    }
                }

                /*
                 * Legs can never point into neighboring edge when convex
                 * vertex, take cutoff-line of neighboring edge instead. If
                 * velocity projected on "foreign" leg, no constraint is added.
                 */

                Obstacle leftNeighbor = obstacle1.previous_;

                bool isLeftLegForeign = false;
                bool isRightLegForeign = false;

                if (obstacle1.convex_ && RVOMath.det(leftLegDirection, -leftNeighbor.direction_) >= 0)
                {
                    /* Left leg points into obstacle. */
                    leftLegDirection = -leftNeighbor.direction_;
                    if (isover(leftLegDirection) )
                    {
                        Debug.LogError("!!!");
                    }
                    isLeftLegForeign = true;
                }

                if (obstacle2.convex_ && RVOMath.det(rightLegDirection, obstacle2.direction_) <= 0)
                {
                    /* Right leg points into obstacle. */
                    rightLegDirection = obstacle2.direction_;
                    isRightLegForeign = true;
                    if ( isover(rightLegDirection))
                    {
                        Debug.LogError("!!!");
                    }
                }

                /* Compute cut-off centers. */
                Jint2 leftCutOff =(obstacle1.point_ - position_) / timeHorizonObst_;
                Jint2 rightCutOff = (obstacle2.point_ - position_) / timeHorizonObst_;
                Jint2 cutOffVector = rightCutOff - leftCutOff;

                /* Project current velocity on velocity obstacle. */

                /* Check if current velocity is projected on cutoff circles. */
                JInt sqvalue = RVOMath.absSq(cutOffVector);
                JInt t = JInt.ToInt(JInt.divscale / 2);
                if(obstacle1 != obstacle2)
                {
                    if (sqvalue == 0)
                    {
                        t = JInt.MaxValue;
                    }
                    else
                    {
                        t = RVOMath.Dot((velocity_ - leftCutOff), cutOffVector) / sqvalue;
                    }
                }

                JInt tLeft = RVOMath.Dot((velocity_ - leftCutOff) , leftLegDirection);
                JInt tRight = RVOMath.Dot((velocity_ - rightCutOff) , rightLegDirection);

                if ((t < 0 && tLeft < 0) || (obstacle1 == obstacle2 && tLeft < 0 && tRight < 0))
                {
                    /* Project on left cut-off circle. */
                    Jint2 unitW = RVOMath.normalize((velocity_ - leftCutOff));

                    line.direction = Jint2.ToInt2(unitW.IntY, -unitW.IntX);
                    line.point = leftCutOff + radius_ *  unitW / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }
                else if (t > 1 && tRight < 0)
                {
                    /* Project on right cut-off circle. */
                    Jint2 unitW = RVOMath.normalize((velocity_ - rightCutOff));

                    line.direction = Jint2.ToInt2(unitW.IntY, -unitW.IntX);
                    line.point = rightCutOff + radius_ *  unitW / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                /*
                 * Project on left leg, right leg, or cut-off line, whichever is
                 * closest to velocity.
                 */
                JInt distSqCutoff = (t < 0 || t > 1 || obstacle1 == obstacle2) ? JInt.MaxValue : RVOMath.absSq(velocity_ - (leftCutOff + t * cutOffVector));
                JInt distSqLeft = tLeft < 0 ? JInt.MaxValue : RVOMath.absSq(velocity_ - (leftCutOff + tLeft * leftLegDirection));
                JInt distSqRight = tRight < 0 ? JInt.MaxValue : RVOMath.absSq(velocity_ - (rightCutOff + tRight * rightLegDirection));

                if (distSqCutoff <= distSqLeft && distSqCutoff <= distSqRight)
                {
                    /* Project on cut-off line. */
                    line.direction = -obstacle1.direction_;
                    line.point = leftCutOff + radius_  * Jint2.ToInt2(-line.direction.IntY, line.direction.IntX) / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                if (distSqLeft <= distSqRight)
                {
                    /* Project on left leg. */
                    if (isLeftLegForeign)
                    {
                        continue;
                    }

                    line.direction = leftLegDirection;
                    line.point = leftCutOff + radius_ * Jint2.ToInt2(-line.direction.IntY, line.direction.IntX) / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                /* Project on right leg. */
                if (isRightLegForeign)
                {
                    continue;
                }

                line.direction = -rightLegDirection;
                line.point = rightCutOff + radius_ * Jint2.ToInt2(-line.direction.IntY, line.direction.IntX) / timeHorizonObst_;
                orcaLines_.Add(line);
            }

            int numObstLines = orcaLines_.Count;

            //KInt invTimeHorizon = 1 / timeHorizon_;

            /* Create agent ORCA lines. */
            for (int i = 0; i < agentNeighbors_.Count; ++i)
            {
                Agent other = agentNeighbors_[i].Value;

                Jint2 relativePosition = other.position_ - position_;
                Jint2 relativeVelocity = velocity_ - other.velocity_;
                JInt distSq = RVOMath.absSq(relativePosition);
                JInt combinedRadius = radius_ + other.radius_;
                JInt combinedRadiusSq = RVOMath.sqr(combinedRadius);

                Line line = new Line();
                Jint2 u;

                if (distSq > combinedRadiusSq)
                {
                    /* No collision. */
                    Jint2 w = relativeVelocity -  relativePosition / timeHorizon_;
                    /* Vector from cutoff center to relative velocity. */
                    JInt wLengthSq = RVOMath.absSq(w);

                    JInt dotProduct1 = RVOMath.Dot( w , relativePosition);

                    if (dotProduct1 < 0 && RVOMath.sqr(dotProduct1) > combinedRadiusSq * wLengthSq)
                    {
                        /* Project on cut-off circle. */
                        JInt wLength = RVOMath.sqrt(wLengthSq);
                        if (wLength == 0)
                            continue;
                        Jint2 unitW = w / wLength;

                        line.direction = Jint2.ToInt2(unitW.IntY, -unitW.IntX);
                        u = (combinedRadius / timeHorizon_ - wLength) * unitW;
                    }
                    else
                    {
                        /* Project on legs. */
                        JInt leg = RVOMath.sqrt(distSq - combinedRadiusSq);

                        if (RVOMath.det(relativePosition, w) > 0)
                        {
                            /* Project on left leg. */
                            line.direction = Jint2.ToInt2(relativePosition.IntX * leg - relativePosition.IntY * combinedRadius, relativePosition.IntX * combinedRadius + relativePosition.IntY * leg) / distSq;
                        }
                        else
                        {
                            /* Project on right leg. */
                            line.direction = -Jint2.ToInt2(relativePosition.IntX * leg + relativePosition.IntY * combinedRadius, -relativePosition.IntX * combinedRadius + relativePosition.IntY * leg) / distSq;
                        }

                        JInt dotProduct2 = RVOMath.Dot( relativeVelocity ,line.direction);
                        u = dotProduct2 * line.direction - relativeVelocity;
                    }
                }
                else
                {
                    /* Collision. Project on cut-off circle of time timeStep. */
                    //KInt invTimeStep = 1 / Simulator.Instance.timeStep_;
                    /* Vector from cutoff center to relative velocity. */
                    Jint2 w = relativeVelocity - relativePosition / Simulator.Instance.timeStep_;
                    JInt wLength = RVOMath.abs(w);
                    if (wLength == 0)
                        continue;
                    Jint2 unitW = w / wLength;
  
                    line.direction = Jint2.ToInt2(unitW.IntY, -unitW.IntX);

                    u = (combinedRadius / Simulator.Instance.timeStep_ - wLength) * unitW;

                }

                line.point = velocity_ +  u /2;
                orcaLines_.Add(line);
            }

            int lineFail = linearProgram2(orcaLines_, maxSpeed_, prefVelocity_, false, ref newVelocity_);
            if (lineFail < orcaLines_.Count)
            {
                linearProgram3(orcaLines_, numObstLines, lineFail, maxSpeed_, ref newVelocity_);
            }

        }

        /**
         * <summary>Inserts an agent neighbor into the set of neighbors of this
         * agent.</summary>
         *
         * <param name="agent">A pointer to the agent to be inserted.</param>
         * <param name="rangeSq">The squared range around this agent.</param>
         */
        internal void insertAgentNeighbor(Agent agent, ref JInt rangeSq)
        {
            if (this != agent)
            {
                JInt distSq = RVOMath.absSq(position_ - agent.position_);

                if (distSq < rangeSq)
                {
                    if (agentNeighbors_.Count < maxNeighbors_)
                    {
                        agentNeighbors_.Add(new KeyValuePair<JInt, Agent>(distSq, agent));
                    }

                    int i = agentNeighbors_.Count - 1;

                    while (i != 0 && distSq < agentNeighbors_[i - 1].Key)
                    {
                        agentNeighbors_[i] = agentNeighbors_[i - 1];
                        --i;
                    }

                    agentNeighbors_[i] = new KeyValuePair<JInt, Agent>(distSq, agent);

                    if (agentNeighbors_.Count == maxNeighbors_)
                    {
                        rangeSq = agentNeighbors_[agentNeighbors_.Count - 1].Key;
                    }
                }
            }
        }

        /**
         * <summary>Inserts a static obstacle neighbor into the set of neighbors
         * of this agent.</summary>
         *
         * <param name="obstacle">The number of the static obstacle to be
         * inserted.</param>
         * <param name="rangeSq">The squared range around this agent.</param>
         */
        internal void insertObstacleNeighbor(Obstacle obstacle, JInt rangeSq)
        {
            Obstacle nextObstacle = obstacle.next_;

            JInt distSq = RVOMath.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, position_);

            if (distSq < rangeSq)
            {
                obstacleNeighbors_.Add(new KeyValuePair<JInt, Obstacle>(distSq, obstacle));

                int i = obstacleNeighbors_.Count - 1;

                while (i != 0 && distSq < obstacleNeighbors_[i - 1].Key)
                {
                    obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
                    --i;
                }
                obstacleNeighbors_[i] = new KeyValuePair<JInt, Obstacle>(distSq, obstacle);
            }
        }

        /**
         * <summary>Updates the two-dimensional position and two-dimensional
         * velocity of this agent.</summary>
         */
        internal void update()
        {
            velocity_ = newVelocity_;
            position_ += velocity_ * Simulator.Instance.timeStep_;
        }

        /**
         * <summary>Solves a one-dimensional linear program on a specified line
         * subject to linear constraints defined by lines and a circular
         * constraint.</summary>
         *
         * <returns>True if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="lineNo">The specified line constraint.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private bool linearProgram1(IList<Line> lines, int lineNo, JInt radius, Jint2 optVelocity, bool directionOpt, ref Jint2 result)
        {
            JInt dotProduct = RVOMath.Dot( lines[lineNo].point , lines[lineNo].direction);
            JInt discriminant = RVOMath.sqr(dotProduct) + RVOMath.sqr(radius) - RVOMath.absSq(lines[lineNo].point);

            if (discriminant < 0)
            {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            JInt sqrtDiscriminant = RVOMath.sqrt(discriminant);
            JInt tLeft = -dotProduct - sqrtDiscriminant;
            JInt tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {
                JInt denominator = RVOMath.det(lines[lineNo].direction, lines[i].direction);
                JInt numerator = RVOMath.det(lines[i].direction, lines[lineNo].point - lines[i].point);

                if (RVOMath.fabs(denominator) <= 0)
                {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < 0)
                    {
                        return false;
                    }

                    continue;
                }

                JInt t = numerator / denominator;

                if (denominator >= 0)
                {
                    /* Line i bounds line lineNo on the right. */
                    tRight = JInt.Min(tRight, t);
                }
                else
                {
                    /* Line i bounds line lineNo on the left. */
                    tLeft = JInt.Max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (RVOMath.Dot( optVelocity, lines[lineNo].direction) > 0)
                {
                    /* Take right extreme. */
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    /* Take left extreme. */
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
            }
            else
            {
                /* Optimize closest point. */
                JInt t = RVOMath.Dot(lines[lineNo].direction ,(optVelocity - lines[lineNo].point));

                if (t < tLeft)
                {
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
                else if (t > tRight)
                {
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    result = lines[lineNo].point + t * lines[lineNo].direction;
                }
            }

            return true;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <returns>The number of the line it fails on, and the number of lines
         * if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private int linearProgram2(IList<Line> lines, JInt radius, Jint2 optVelocity, bool directionOpt, ref Jint2 result)
        {
            if (directionOpt)
            {
                /*
                 * Optimize direction. Note that the optimization velocity is of
                 * unit length in this case.
                 */
                result = optVelocity * radius;
            }
            else if (RVOMath.absSq(optVelocity) > RVOMath.sqr(radius))
            {
                /* Optimize closest point and outside circle. */
                result = RVOMath.normalize(optVelocity ) * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }
            for (int i = 0; i < lines.Count; ++i)
            {
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > 0)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Jint2 tempResult = result;
                    if (!linearProgram1(lines, i, radius, optVelocity, directionOpt, ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return lines.Count;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="numObstLines">Count of obstacle lines.</param>
         * <param name="beginLine">The line on which the 2-d linear program
         * failed.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private void linearProgram3(IList<Line> lines, int numObstLines, int beginLine, JInt radius, ref Jint2 result)
        {
            JInt distance = 0;

            for (int i = beginLine; i < lines.Count; ++i)
            {
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > distance)
                {
                    /* Result does not satisfy constraint of line i. */
                    IList<Line> projLines = new List<Line>();
                    for (int ii = 0; ii < numObstLines; ++ii)
                    {
                        projLines.Add(lines[ii]);
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {
                        Line line = new Line();

                        JInt determinant = RVOMath.det(lines[i].direction, lines[j].direction);
                        if (RVOMath.fabs(determinant) <= 0)
                        {
                            /* Line i and line j are parallel. */
                            if ( RVOMath.Dot( lines[i].direction , lines[j].direction) > 0)
                            {
                                /* Line i and line j point in the same direction. */

                                continue;
                            }
                            else
                            {
                                /* Line i and line j point in opposite direction. */
                                line.point = (lines[i].point + lines[j].point)/2;
                            }
                        }
                        else
                        {
                            line.point = lines[i].point + (RVOMath.det(lines[j].direction, lines[i].point - lines[j].point) / determinant) * lines[i].direction;
                        }
                        line.direction = RVOMath.normalize((lines[j].direction - lines[i].direction));
                        projLines.Add(line);
                    }
                    Jint2 tempResult = result;
                    if (linearProgram2(projLines, radius, Jint2.ToInt2(-lines[i].direction.IntY, lines[i].direction.IntX), true, ref result) < projLines.Count)
                    {
                        /*
                         * This should in principle not happen. The result is by
                         * definition already in the feasible region of this
                         * linear program. If it fails, it is due to small
                         * floating point error, and the current result is kept.
                         */
                        result = tempResult;
                    }

                    distance = RVOMath.det(lines[i].direction, lines[i].point - result);
                }
            }
        }
    }
}
