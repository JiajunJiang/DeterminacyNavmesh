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
using DefaultNamespace;
using UnityEngine;

namespace RVO {
    /**
     * <summary>Defines an agent in the simulation.</summary>
     */
    internal class Agent {
        static bool msDirectionOpt = false;

        internal IList<KeyValuePair<long, Agent>> agentNeighbors_ = new List<KeyValuePair<long, Agent>>();
        internal IList<KeyValuePair<long, Obstacle>> obstacleNeighbors_ = new List<KeyValuePair<long, Obstacle>>();
        internal IList<Line> orcaLines_ = new List<Line>();
        internal Point2D position_;
        internal Point2D prefVelocity_;
        internal Point2D velocity_;
        internal int id_ = 0;
        internal int maxNeighbors_ = 0;
        internal int maxSpeed_ = 0; // float * 100
        internal int neighborDist_ = 0; // float * 100
        internal int radius_ = 0; // float * 100
        internal int timeHorizon_ = 0; // float * 100
        internal int timeHorizonObst_ = 0; // float * 100

        internal int mass_ = 100; // float * 100

        private Point2D newVelocity_;

        /**
         * <summary>Computes the neighbors of this agent.</summary>
         */
        internal void computeNeighbors() {
            obstacleNeighbors_.Clear();
            long rangeSq = RVOMath.sqr(timeHorizonObst_ * maxSpeed_ + radius_);
            Simulator.Instance.kdTree_.computeObstacleNeighbors(this, rangeSq);

            agentNeighbors_.Clear();

            if (maxNeighbors_ > 0) {
                rangeSq = RVOMath.sqr(neighborDist_);
                Simulator.Instance.kdTree_.computeAgentNeighbors(this, ref rangeSq);
            }
        }

        /**
         * <summary>Computes the new velocity of this agent.</summary>
         */
        internal void computeNewVelocity() {
            orcaLines_.Clear();

            /* Create obstacle ORCA lines. */
            for (int i = 0; i < obstacleNeighbors_.Count; ++i) {

                Obstacle obstacle1 = obstacleNeighbors_[i].Value;
                Obstacle obstacle2 = obstacle1.next_;

                Point2D relativePosition1 = obstacle1.point_ - position_;
                Point2D relativePosition2 = obstacle2.point_ - position_;

                /*
                 * Check if velocity obstacle of obstacle is already taken care
                 * of by previously constructed obstacle ORCA lines.
                 */
                bool alreadyCovered = false;

                for (int j = 0; j < orcaLines_.Count; ++j) {
                    if (RVOMath.det( relativePosition1 / timeHorizonObst_ - orcaLines_[j].point, orcaLines_[j].direction) - radius_ / timeHorizonObst_ >= -RVOMath.RVO_EPSILON && RVOMath.det(relativePosition2 / timeHorizonObst_ - orcaLines_[j].point, orcaLines_[j].direction) -  radius_ / timeHorizonObst_ >= -RVOMath.RVO_EPSILON) {
                        alreadyCovered = true;

                        break;
                    }
                }

                if (alreadyCovered) {
                    continue;
                }

                /* Not yet covered. Check for collisions. */
                long distSq1 = RVOMath.absSq(relativePosition1);
                long distSq2 = RVOMath.absSq(relativePosition2);

                long radiusSq = RVOMath.sqr(radius_);

                Point2D obstacleVector = obstacle2.point_ - obstacle1.point_;
                long s = Point2D.Dot(-relativePosition1 , obstacleVector) / RVOMath.absSq(obstacleVector);
                float distSqLine = RVOMath.absSq(-relativePosition1 - obstacleVector * s);

                Line line;

                if (s < 0.0f && distSq1 <= radiusSq) {
                    /* Collision with left vertex. Ignore if non-convex. */
                    if (obstacle1.convex_) {
                        line.point = new Point2D(0, 0);
                        line.direction = RVOMath.normalize(new Point2D(-relativePosition1.y, relativePosition1.x));
                        orcaLines_.Add(line);
                    }

                    continue;
                } else if (s > 1.0f && distSq2 <= radiusSq) {
                    /*
                     * Collision with right vertex. Ignore if non-convex or if
                     * it will be taken care of by neighboring obstacle.
                     */
                    if (obstacle2.convex_ && RVOMath.det(relativePosition2, obstacle2.direction_) >= 0.0f) {
                        line.point = new Point2D(0, 0);
                        line.direction = RVOMath.normalize(new Point2D(-relativePosition2.y, relativePosition2.x));
                        orcaLines_.Add(line);
                    }

                    continue;
                } else if (s >= 0.0f && s < 1.0f && distSqLine <= radiusSq) {
                    /* Collision with obstacle segment. */
                    line.point = new Point2D(0, 0);
                    line.direction = -obstacle1.direction_;
                    orcaLines_.Add(line);

                    continue;
                }

                /*
                 * No collision. Compute legs. When obliquely viewed, both legs
                 * can come from a single vertex. Legs extend cut-off line when
                 * non-convex vertex.
                 */

                Point2D leftLegDirection, rightLegDirection;

                if (s < 0.0f && distSqLine <= radiusSq) {
                    /*
                     * Obstacle viewed obliquely so that left vertex
                     * defines velocity obstacle.
                     */
                    if (!obstacle1.convex_) {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle2 = obstacle1;

                    long leg1 = RVOMath.sqrt(distSq1 - radiusSq);
                    leftLegDirection = new Point2D(relativePosition1.x * leg1 - relativePosition1.y * radius_, relativePosition1.x * radius_ + relativePosition1.y * leg1) / distSq1;
                    rightLegDirection = new Point2D(relativePosition1.x * leg1 + relativePosition1.y * radius_, -relativePosition1.x * radius_ + relativePosition1.y * leg1) / distSq1;
                } else if (s > 1.0f && distSqLine <= radiusSq) {
                    /*
                     * Obstacle viewed obliquely so that
                     * right vertex defines velocity obstacle.
                     */
                    if (!obstacle2.convex_) {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle1 = obstacle2;

                    long leg2 = RVOMath.sqrt(distSq2 - radiusSq);
                    leftLegDirection = new Point2D(relativePosition2.x * leg2 - relativePosition2.y * radius_, relativePosition2.x * radius_ + relativePosition2.y * leg2) / distSq2;
                    rightLegDirection = new Point2D(relativePosition2.x * leg2 + relativePosition2.y * radius_, -relativePosition2.x * radius_ + relativePosition2.y * leg2) / distSq2;
                } else {
                    /* Usual situation. */
                    if (obstacle1.convex_) {
                        long leg1 = RVOMath.sqrt(distSq1 - radiusSq);
                        leftLegDirection = new Point2D(relativePosition1.x * leg1 - relativePosition1.y * radius_, relativePosition1.x * radius_ + relativePosition1.y * leg1) / distSq1;
                    } else {
                        /* Left vertex non-convex; left leg extends cut-off line. */
                        leftLegDirection = -obstacle1.direction_;
                    }

                    if (obstacle2.convex_) {
                        long leg2 = RVOMath.sqrt(distSq2 - radiusSq);
                        rightLegDirection = new Point2D(relativePosition2.x * leg2 + relativePosition2.y * radius_, -relativePosition2.x * radius_ + relativePosition2.y * leg2) / distSq2;
                    } else {
                        /* Right vertex non-convex; right leg extends cut-off line. */
                        rightLegDirection = obstacle1.direction_;
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

                if (obstacle1.convex_ && RVOMath.det(leftLegDirection, -leftNeighbor.direction_) >= 0.0f) {
                    /* Left leg points into obstacle. */
                    leftLegDirection = -leftNeighbor.direction_;
                    isLeftLegForeign = true;
                }

                if (obstacle2.convex_ && RVOMath.det(rightLegDirection, obstacle2.direction_) <= 0.0f) {
                    /* Right leg points into obstacle. */
                    rightLegDirection = obstacle2.direction_;
                    isRightLegForeign = true;
                }

                /* Compute cut-off centers. */
                Point2D leftCutOff =  (obstacle1.point_ - position_) / timeHorizonObst_ ;
                Point2D rightCutOff = (obstacle2.point_ - position_) / timeHorizonObst_ ;
                Point2D cutOffVector = rightCutOff - leftCutOff;

                /* Project current velocity on velocity obstacle. */

                /* Check if current velocity is projected on cutoff circles. */
                long t = obstacle1 == obstacle2 ? 1 : (Point2D.Dot((velocity_ - leftCutOff) , cutOffVector)) / RVOMath.absSq(cutOffVector) * 2;
                long tLeft = Point2D.Dot((velocity_ - leftCutOff) , leftLegDirection);
                long tRight = Point2D.Dot((velocity_ - rightCutOff) , rightLegDirection);

                if ((t < 0 && tLeft < 0) || (obstacle1 == obstacle2 && tLeft < 0 && tRight < 0)) {
                    /* Project on left cut-off circle. */
                    Point2D unitW = RVOMath.normalize(velocity_ - leftCutOff);

                    line.direction = new Point2D(unitW.y, -unitW.x);
                    line.point = leftCutOff +   unitW * radius_ / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                } else if (t > 1 * 2 && tRight < 0f) {
                    /* Project on right cut-off circle. */
                    Point2D unitW = RVOMath.normalize(velocity_ - rightCutOff);

                    line.direction = new Point2D(unitW.y, -unitW.x);
                    line.point = rightCutOff + unitW * radius_ / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                /*
                 * Project on left leg, right leg, or cut-off line, whichever is
                 * closest to velocity.
                 */
                float distSqCutoff = (t < 0 || t > 1 * 2 || obstacle1 == obstacle2) ? float.PositiveInfinity : RVOMath.absSq(velocity_ - (leftCutOff + cutOffVector * t / 2));
                float distSqLeft = tLeft < 0.0f ? float.PositiveInfinity : RVOMath.absSq(velocity_ - (leftCutOff +  leftLegDirection * tLeft));
                float distSqRight = tRight < 0.0f ? float.PositiveInfinity : RVOMath.absSq(velocity_ - (rightCutOff + rightLegDirection * tRight));

                if (distSqCutoff <= distSqLeft && distSqCutoff <= distSqRight) {
                    /* Project on cut-off line. */
                    line.direction = -obstacle1.direction_;
                    line.point = leftCutOff +  new Point2D(-line.direction.y, line.direction.x) * radius_ / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                if (distSqLeft <= distSqRight) {
                    /* Project on left leg. */
                    if (isLeftLegForeign) {
                        continue;
                    }

                    line.direction = leftLegDirection;
                    line.point = leftCutOff + new Point2D(-line.direction.y, line.direction.x) * radius_ / timeHorizonObst_;
                    orcaLines_.Add(line);

                    continue;
                }

                /* Project on right leg. */
                if (isRightLegForeign) {
                    continue;
                }

                line.direction = -rightLegDirection;
                line.point = rightCutOff + new Point2D(-line.direction.y, line.direction.x) *  radius_ / timeHorizonObst_;
                orcaLines_.Add(line);
            }

            int numObstLines = orcaLines_.Count;

            float invTimeHorizon = 1.0f / timeHorizon_;

            /* Create agent ORCA lines. */
            for (int i = 0; i < agentNeighbors_.Count; ++i) {


                Agent other = agentNeighbors_[i].Value;

                Point2D relativePosition = other.position_ - position_;

                // mass
                int massRatio = (other.mass_ / (mass_ + other.mass_));
                int neighborMassRatio = (mass_ / (mass_ + other.mass_));
                //massRatio = 0.5f;
                //neighborMassRatio = 0.5f;
                Point2D velocityOpt = (massRatio * 2 >= 1 ? (velocity_ - velocity_ * massRatio) * 2 : prefVelocity_ + (velocity_ - prefVelocity_) * massRatio * 2);
                Point2D neighborVelocityOpt = (neighborMassRatio * 2 >= 1 ? other.velocity_ * 2 * (1 - neighborMassRatio) : other.prefVelocity_ + (other.velocity_ - other.prefVelocity_) * neighborMassRatio * 2); ;

                //massRatio = 0.5f;
                //velocityOpt = velocity_;
                //neighborVelocityOpt = other.velocity_;

                Point2D relativeVelocity = velocityOpt - neighborVelocityOpt;
                long distSq = RVOMath.absSq(relativePosition);
                long combinedRadius = radius_ + other.radius_;
                if (mass_ != other.mass_) {
                    //combinedRadius = combinedRadius * 0.45f;
                }
                long combinedRadiusSq = RVOMath.sqr(combinedRadius);

                Line line;
                Point2D u;

                if (distSq > combinedRadiusSq) {
                    /* No collision. */
                    Point2D w = relativeVelocity - relativePosition / timeHorizon_;

                    /* Vector from cutoff center to relative velocity. */
                    long wLengthSq = RVOMath.absSq(w);
                    long dotProduct1 = Point2D.Dot( w , relativePosition);

                    if (dotProduct1 < 0.0f && RVOMath.sqr(dotProduct1) > combinedRadiusSq * wLengthSq) {
                        /* Project on cut-off circle. */
                        long wLength = RVOMath.sqrt(wLengthSq);
                        Point2D unitW = w / wLength;

                        line.direction = new Point2D(unitW.y, -unitW.x);
                        u =  unitW *(combinedRadius / timeHorizon_ - wLength);
                    } else {
                        /* Project on legs. */
                        long leg = RVOMath.sqrt(distSq - combinedRadiusSq);

                        if (RVOMath.det(relativePosition, w) > 0.0f) {
                            /* Project on left leg. */
                            line.direction = new Point2D(relativePosition.x * leg - relativePosition.y * combinedRadius, relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                        } else {
                            /* Project on right leg. */
                            line.direction = -new Point2D(relativePosition.x * leg + relativePosition.y * combinedRadius, -relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                        }

                        long dotProduct2 = Point2D.Dot(relativeVelocity , line.direction);
                        u = line.direction * dotProduct2 - relativeVelocity;
                    }
                } else {
                    /* Vector from cutoff center to relative velocity. */
                    Point2D w = relativeVelocity - relativePosition / Simulator.Instance.timeStep_;

                    long wLength = RVOMath.abs(w);
                    Point2D unitW = w / wLength;

                    line.direction = new Point2D(unitW.y, -unitW.x);
                    u =  unitW * (combinedRadius / Simulator.Instance.timeStep_ - wLength);
                }

                //line.point = velocityOpt + 0.5f * u;
                line.point = velocityOpt + u * massRatio;
                orcaLines_.Add(line);

                ////////////////////////////////////////////////////////////////////////////////////////////////
                // Test01.mSphereScritps[id_].msGizmosLines.Add(line);
                ////////////////////////////////////////////////////////////////////////////////////////////////
            }

            if (mass_ != 1) {
                int i = 1;
            }

            int lineFail = linearProgram2(orcaLines_, maxSpeed_, prefVelocity_, msDirectionOpt, ref newVelocity_);

            if (lineFail < orcaLines_.Count) {

                if (mass_ != 1) {
                    int i = 1;
                }

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
        internal void insertAgentNeighbor(Agent agent, ref long rangeSq) {
            if (this != agent) {
                long distSq = RVOMath.absSq(position_ - agent.position_);

                if (distSq < rangeSq) {
                    if (agentNeighbors_.Count < maxNeighbors_) {
                        agentNeighbors_.Add(new KeyValuePair<long, Agent>(distSq, agent));
                    }

                    int i = agentNeighbors_.Count - 1;

                    while (i != 0 && distSq < agentNeighbors_[i - 1].Key) {
                        agentNeighbors_[i] = agentNeighbors_[i - 1];
                        --i;
                    }

                    agentNeighbors_[i] = new KeyValuePair<long, Agent>(distSq, agent);

                    if (agentNeighbors_.Count == maxNeighbors_) {
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
        internal void insertObstacleNeighbor(Obstacle obstacle, long rangeSq) {
            Obstacle nextObstacle = obstacle.next_;

            long distSq = RVOMath.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, position_);

            if (distSq < rangeSq) {
                obstacleNeighbors_.Add(new KeyValuePair<long, Obstacle>(distSq, obstacle));

                int i = obstacleNeighbors_.Count - 1;

                while (i != 0 && distSq < obstacleNeighbors_[i - 1].Key) {
                    obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
                    --i;
                }
                obstacleNeighbors_[i] = new KeyValuePair<long, Obstacle>(distSq, obstacle);
            }
        }

        /**
         * <summary>Updates the two-dimensional position and two-dimensional
         * velocity of this agent.</summary>
         */
        internal void update() {
            velocity_ = newVelocity_;
            position_ += velocity_ * Simulator.Instance.timeStep_;

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Test01.mSphereScritps[id_].msVelocity = velocity_;
            ////////////////////////////////////////////////////////////////////////////////////////////////
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
        private bool linearProgram1(IList<Line> lines, int lineNo, long radius, Point2D optVelocity, bool directionOpt, ref Point2D result) {
            long dotProduct = Point2D.Dot(lines[lineNo].point , lines[lineNo].direction);
            long discriminant = RVOMath.sqr(dotProduct) + RVOMath.sqr(radius) - RVOMath.absSq(lines[lineNo].point);

            if (discriminant < 0.0f) {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            long sqrtDiscriminant = RVOMath.sqrt(discriminant);
            long tLeft = -dotProduct - sqrtDiscriminant;
            long tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i) {
                long denominator = RVOMath.det(lines[lineNo].direction, lines[i].direction);
                long numerator = RVOMath.det(lines[i].direction, lines[lineNo].point - lines[i].point);

                if (RVOMath.longAbs(denominator) <= RVOMath.RVO_EPSILON) {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < 0.0f) {
                        return false;
                    }

                    continue;
                }

                long t = numerator / denominator;

                if (denominator >= 0.0f) {
                    /* Line i bounds line lineNo on the right. */
                    tRight = Math.Min(tRight, t);
                } else {
                    /* Line i bounds line lineNo on the left. */
                    tLeft = Math.Max(tLeft, t);
                }

                if (tLeft > tRight) {
                    return false;
                }
            }

            if (directionOpt) {
                /* Optimize direction. */
                if (Point2D.Dot(optVelocity , lines[lineNo].direction) > 0) {
                    /* Take right extreme. */
                    result = lines[lineNo].point + lines[lineNo].direction * tRight;
                } else {
                    /* Take left extreme. */
                    result = lines[lineNo].point + lines[lineNo].direction * tLeft;
                }
            } else {
                /* Optimize closest point. */
                long t = Point2D.Dot(lines[lineNo].direction , (optVelocity - lines[lineNo].point));

                if (t < tLeft) {
                    result = lines[lineNo].point + lines[lineNo].direction * tLeft;
                } else if (t > tRight) {
                    result = lines[lineNo].point + lines[lineNo].direction * tRight;
                } else {
                    result = lines[lineNo].point + lines[lineNo].direction * t ;
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
        private int linearProgram2(IList<Line> lines, long radius, Point2D optVelocity, bool directionOpt, ref Point2D result) {
            // directionOpt 第一次为false，第二次为true，directionOpt主要用在 linearProgram1 里面
            if (directionOpt) {
                /*
                 * Optimize direction. Note that the optimization velocity is of
                 * unit length in this case.
                 */
                // 1.这个其实没什么用，只是因为velocity是归一化的所以直接乘 radius
                result = optVelocity * radius;
            } else if (RVOMath.absSq(optVelocity) > RVOMath.sqr(radius)) {
                /* Optimize closest point and outside circle. */
                // 2.当 optVelocity 太大时，先归一化optVelocity，再乘 radius
                result = RVOMath.normalize(optVelocity) * radius;
            } else {
                /* Optimize closest point and inside circle. */
                // 3.当 optVelocity 小于maxSpeed时
                result = optVelocity;
            }

            for (int i = 0; i < lines.Count; ++i) {
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > 0.0f) {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Point2D tempResult = result;
                    if (!linearProgram1(lines, i, radius, optVelocity, directionOpt, ref result)) {
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
        private void linearProgram3(IList<Line> lines, int numObstLines, int beginLine, long radius, ref Point2D result) {

            if (mass_ != 1) {
                Debug.Log("linearProgram3 beginLine:"+ beginLine);
            }

            float distance = 0.0f;
            // 遍历所有剩余ORCA线
            for (int i = beginLine; i < lines.Count; ++i) {
                // 每一条 ORCA 线都需要精确的做出处理，distance 为 最大违规的速度
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > distance) {
                    /* Result does not satisfy constraint of line i. */
                    IList<Line> projLines = new List<Line>();
                    // 1.静态阻挡的orca线直接加到projLines中
                    for (int ii = 0; ii < numObstLines; ++ii) {
                        projLines.Add(lines[ii]);
                    }
                    // 2.动态阻挡的orca线需要重新计算line，从第一个非静态阻挡到当前的orca线
                    for (int j = numObstLines; j < i; ++j) {
                        Line line;

                        long determinant = RVOMath.det(lines[i].direction, lines[j].direction);

                        if (RVOMath.longAbs(determinant) <= RVOMath.RVO_EPSILON) {
                            /* Line i and line j are parallel. */
                            if (Point2D.Dot(lines[i].direction , lines[j].direction) > 0) {
                                /* Line i and line j point in the same direction. */
                                // 2-1 两条线平行且同向
                                continue;
                            } else {
                                /* Line i and line j point in opposite direction. */
                                // 2-2 两条线平行且反向
                                line.point = (lines[i].point + lines[j].point) / 2;
                            }
                        } else {
                            // 2-3 两条线不平行
                            line.point = lines[i].point + lines[i].direction * RVOMath.det(lines[j].direction, lines[i].point - lines[j].point) / determinant;
                        }
                        // 计算ORCA线的方向
                        line.direction = RVOMath.normalize(lines[j].direction - lines[i].direction);
                        projLines.Add(line);
                    }
                    // 3.再次计算最优速度
                    Point2D tempResult = result;
                    // 注意这里的 new Vector2(-lines[i].direction.y, lines[i].direction.x) 是方向向量
                    if (linearProgram2(projLines, radius, new Point2D(-lines[i].direction.y, lines[i].direction.x), true, ref result) < projLines.Count) {
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
