/*
 * KdTree.cs
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

using System.Collections.Generic;
using System;
using FixedMath;

namespace RVO
{
    /**
     * <summary>Defines k-D trees for agents and static obstacles in the
     * simulation.</summary>
     */
    public class KdTree
    {
        /**
         * <summary>Defines a node of an agent k-D tree.</summary>
         */
        private struct AgentTreeNode
        {
            internal int begin_;
            internal int end_;
            internal int left_;
            internal int right_;
            internal JInt maxx;
            internal JInt maxy;
            internal JInt minx;
            internal JInt miny;
        }
            
        /**
         * <summary>Defines a node of an obstacle k-D tree.</summary>
         */
        internal class ObstacleTreeNode
        {
            internal Obstacle obstacle_;
            internal ObstacleTreeNode left_;
            internal ObstacleTreeNode right_;

        };

        /**
         * <summary>The maximum size of an agent k-D tree leaf.</summary>
         */
        private const int MAX_LEAF_SIZE = 32;

        private Agent[] agents_;
        private AgentTreeNode[] agentTree_;
        internal ObstacleTreeNode obstacleTree_;


        /**
         * <summary>Builds an agent k-D tree.</summary>
         */
        internal void buildAgentTree()
        {
            if (agents_ == null || agents_.Length != Simulator.Instance.agents_.Count)
            {
                agents_ = new Agent[Simulator.Instance.agents_.Count];

                for (int i = 0; i < agents_.Length; ++i)
                {
                    agents_[i] = Simulator.Instance.agents_[i];
                }

                agentTree_ = new AgentTreeNode[2 * agents_.Length];

                for (int i = 0; i < agentTree_.Length; ++i)
                {
                    agentTree_[i] = new AgentTreeNode();
                }
            }

            if (agents_.Length != 0)
            {
                buildAgentTreeRecursive(0, agents_.Length, 0);
            }
        }

        /**
         * <summary>Builds an obstacle k-D tree.</summary>
         */
        internal void buildObstacleTree()
        {

            IList<Obstacle> obstacles = new List<Obstacle>(Simulator.Instance.obstacles_.Count);

            for (int i = 0; i < Simulator.Instance.obstacles_.Count; ++i)
            {
                obstacles.Add(Simulator.Instance.obstacles_[i]);
            }

            obstacleTree_ = buildObstacleTreeRecursive(obstacles);
        }

        /**
         * <summary>Computes the agent neighbors of the specified agent.
         * </summary>
         *
         * <param name="agent">The agent for which agent neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         */
        internal void computeAgentNeighbors(Agent agent, ref JInt rangeSq)
        {
            queryAgentTreeRecursive(agent, ref rangeSq, 0);
        }

        /**
         * <summary>Computes the obstacle neighbors of the specified agent.
         * </summary>
         *
         * <param name="agent">The agent for which obstacle neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         */
        internal void computeObstacleNeighbors(Agent agent, JInt rangeSq)
        {
            queryObstacleTreeRecursive(agent, rangeSq, obstacleTree_);
        }

        /**
         * <summary>Queries the visibility between two points within a specified
         * radius.</summary>
         *
         * <returns>True if q1 and q2 are mutually visible within the radius;
         * false otherwise.</returns>
         *
         * <param name="q1">The first point between which visibility is to be
         * tested.</param>
         * <param name="q2">The second point between which visibility is to be
         * tested.</param>
         * <param name="radius">The radius within which visibility is to be
         * tested.</param>
         */
        internal bool queryVisibility(Jint2 q1, Jint2 q2, JInt radius)
        {
            return queryVisibilityRecursive(q1, q2, radius, obstacleTree_);
        }

        internal int queryNearAgent(Jint2 point, JInt radius)
        {
            JInt rangeSq = JInt.MaxValue;
            int agentNo = -1;
            queryAgentTreeRecursive(point, ref rangeSq, ref agentNo, 0);
            if (rangeSq < radius*radius)
                return agentNo;
            return -1;
        }

        private static JInt ReduceMax(JInt value,long right)
        {
            right = right * JInt.divscale / Jint2.divscale;
            JInt data = JInt.ToInt(value.IntValue - right);
            if(data <= 0)
            {
                return JInt.Zero;
            }
            return data;
        }

        private static JInt ReduceMax( long left, JInt value)
        {
            left = left * JInt.divscale / Jint2.divscale;

            JInt data = JInt.ToInt( left - value.IntValue);
            if (data <= 0)
            {
                return JInt.Zero;
            }
            return data;
        }

        private static JInt Max(JInt left,long right)
        {
            right = right * JInt.divscale / Jint2.divscale;
            if(left.IntValue > right)
            {
                return left;
            }

            return JInt.ToInt(right);
        }

        private static JInt Min(JInt left, long right)
        {
            right = right * JInt.divscale / Jint2.divscale;
            if (left.IntValue < right )
            {
                return left;
            }

            return JInt.ToInt(right);
        }

        /**
         * <summary>Recursive method for building an agent k-D tree.</summary>
         *
         * <param name="begin">The beginning agent k-D tree node node index.
         * </param>
         * <param name="end">The ending agent k-D tree node index.</param>
         * <param name="node">The current agent k-D tree node index.</param>
         */
        private void buildAgentTreeRecursive(int begin, int end, int node)
        {
            agentTree_[node].begin_ = begin;
            agentTree_[node].end_ = end;
            agentTree_[node].minx = agentTree_[node].maxx = JInt.ToInt(agents_[begin].position_.IntX * JInt.divscale / Jint2.divscale);
            agentTree_[node].miny = agentTree_[node].maxy = JInt.ToInt(agents_[begin].position_.IntY * JInt.divscale / Jint2.divscale);

            for (int i = begin + 1; i < end; ++i)
            {
                agentTree_[node].maxx = Max(agentTree_[node].maxx, agents_[i].position_.IntX);
                agentTree_[node].minx = Min(agentTree_[node].minx, agents_[i].position_.IntX);
                agentTree_[node].maxy = Max(agentTree_[node].maxy, agents_[i].position_.IntY);
                agentTree_[node].miny = Min(agentTree_[node].miny, agents_[i].position_.IntY);
            }

            if (end - begin > MAX_LEAF_SIZE)
            {
                /* No leaf node. */
                bool isVertical = agentTree_[node].maxx - agentTree_[node].minx > agentTree_[node].maxy - agentTree_[node].miny;
                JInt splitValue = (isVertical ? agentTree_[node].maxx + agentTree_[node].minx : agentTree_[node].maxy + agentTree_[node].miny) /2;
                long convertvalue = splitValue.IntValue * Jint2.divscale / JInt.divscale;

                int left = begin;
                int right = end;

                while (left < right)
                {
                    while (left < right && (isVertical ? agents_[left].position_.IntX : agents_[left].position_.IntY) < convertvalue)
                    {
                        ++left;
                    }

                    while (right > left && (isVertical ? agents_[right - 1].position_.IntX : agents_[right - 1].position_.IntY) >= convertvalue)
                    {
                        --right;
                    }

                    if (left < right)
                    {
                        Agent tempAgent = agents_[left];
                        agents_[left] = agents_[right - 1];
                        agents_[right - 1] = tempAgent;
                        ++left;
                        --right;
                    }
                }

                int leftSize = left - begin;

                if (leftSize == 0)
                {
                    ++leftSize;
                    ++left;
                    ++right;
                }

                agentTree_[node].left_ = node + 1;
                agentTree_[node].right_ = node + 2 * leftSize;

                buildAgentTreeRecursive(begin, left, agentTree_[node].left_);
                buildAgentTreeRecursive(left, end, agentTree_[node].right_);
            }
        }

        public static bool Less(int a1,int b1,int a2,int b2)
        {
            return a1< a2|| !(a2< a1) && b1 < b2;
        }

        /**
         * <summary>Recursive method for building an obstacle k-D tree.
         * </summary>
         *
         * <returns>An obstacle k-D tree node.</returns>
         *
         * <param name="obstacles">A list of obstacles.</param>
         */
        private ObstacleTreeNode buildObstacleTreeRecursive(IList<Obstacle> obstacles)
        {
            if (obstacles.Count == 0)
            {
                return null;
            }

            ObstacleTreeNode node = new ObstacleTreeNode();

            int optimalSplit = 0;
            int minLeft = obstacles.Count;
            int minRight = obstacles.Count;

            for (int i = 0; i < obstacles.Count; ++i)
            {
                int leftSize = 0;
                int rightSize = 0;

                Obstacle obstacleI1 = obstacles[i];
                Obstacle obstacleI2 = obstacleI1.next_;

                /* Compute optimal split node. */

                for (int j = 0; j < obstacles.Count; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = obstacles[j];
                    Obstacle obstacleJ2 = obstacleJ1.next_;

                    JInt j1LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    JInt j2LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);

                    if (j1LeftOfI >= 0 && j2LeftOfI >=0)
                    {
                        ++leftSize;
                    }
                    else if (j1LeftOfI <= 0 && j2LeftOfI <= 0)
                    {
                        ++rightSize;
                    }
                    else
                    {
                        ++leftSize;
                        ++rightSize;
                    }
                    if (!Less(Math.Max(leftSize, rightSize),Math.Min(leftSize, rightSize),Math.Max(minLeft, minRight),Math.Min(minLeft, minRight)))
                    {
                        break;
                    }
                }

                if (Less(Math.Max(leftSize, rightSize),Math.Min(leftSize, rightSize),Math.Max(minLeft, minRight), Math.Min(minLeft, minRight)) )
                {
                    minLeft = leftSize;
                    minRight = rightSize;
                    optimalSplit = i;
                }
            }
            {
                /* Build split node. */
                IList<Obstacle> leftObstacles = new List<Obstacle>(minLeft);

                for (int n = 0; n < minLeft; ++n)
                {
                    leftObstacles.Add(null);
                }

                IList<Obstacle> rightObstacles = new List<Obstacle>(minRight);

                for (int n = 0; n < minRight; ++n)
                {
                    rightObstacles.Add(null);
                }

                int leftCounter = 0;
                int rightCounter = 0;
                int i = optimalSplit;

                Obstacle obstacleI1 = obstacles[i];
                Obstacle obstacleI2 = obstacleI1.next_;

                for (int j = 0; j < obstacles.Count; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = obstacles[j];
                    Obstacle obstacleJ2 = obstacleJ1.next_;

                    JInt j1LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    JInt j2LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);

                    if (j1LeftOfI >= 0 && j2LeftOfI >= 0)
                    {
                        leftObstacles[leftCounter++] = obstacles[j];
                    }
                    else if (j1LeftOfI <= 0 && j2LeftOfI <= 0)
                    {
                        rightObstacles[rightCounter++] = obstacles[j];
                    }
                    else
                    {
                        /* Split obstacle j. */
                        //KInt t = RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) / RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                        Jint2 splitPoint = obstacleJ1.point_ + RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) * (obstacleJ2.point_ - obstacleJ1.point_) / RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                        Obstacle newObstacle = new Obstacle();
                        newObstacle.point_ = splitPoint;
                        newObstacle.previous_ = obstacleJ1;
                        newObstacle.next_ = obstacleJ2;
                        newObstacle.convex_ = true;
                        newObstacle.direction_ = obstacleJ1.direction_;

                        newObstacle.id_ = Simulator.Instance.obstacles_.Count;

                        Simulator.Instance.obstacles_.Add(newObstacle);

                        obstacleJ1.next_ = newObstacle;
                        obstacleJ2.previous_ = newObstacle;

                        if (j1LeftOfI > 0)
                        {
                            leftObstacles[leftCounter++] = obstacleJ1;
                            rightObstacles[rightCounter++] = newObstacle;
                        }
                        else
                        {
                            rightObstacles[rightCounter++] = obstacleJ1;
                            leftObstacles[leftCounter++] = newObstacle;
                        }
                    }
                }

                node.obstacle_ = obstacleI1;
                node.left_ = buildObstacleTreeRecursive(leftObstacles);
                node.right_ = buildObstacleTreeRecursive(rightObstacles);

                return node;
            }
        }

        private void queryAgentTreeRecursive(Jint2 position, ref JInt rangeSq, ref int agentNo, int node)
        {
            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {
                    JInt distSq = RVOMath.absSq(position - agents_[i].position_) ;
                    if (distSq < rangeSq)
                    {
                        rangeSq = distSq;
                        agentNo = agents_[i].id_;
                    }
                }
            }
            else
            {
                JInt distSqLeft = RVOMath.sqr(ReduceMax(agentTree_[agentTree_[node].left_].minx , position.IntX)) + RVOMath.sqr(ReduceMax( position.IntX , agentTree_[agentTree_[node].left_].maxx) ) + RVOMath.sqr(ReduceMax( agentTree_[agentTree_[node].left_].miny , position.IntY)) + RVOMath.sqr(ReduceMax( position.IntY ,agentTree_[agentTree_[node].left_].maxy) );

                JInt distSqRight = RVOMath.sqr(ReduceMax( agentTree_[agentTree_[node].right_].minx, position.IntX)) + RVOMath.sqr(ReduceMax( position.IntX,agentTree_[agentTree_[node].right_].maxx) ) + RVOMath.sqr(ReduceMax(agentTree_[agentTree_[node].right_].miny , position.IntY)) + RVOMath.sqr(ReduceMax( position.IntY , agentTree_[agentTree_[node].right_].maxy));

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].left_);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].right_);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].right_);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].left_);
                        }
                    }
                }

            }
        }

        /**
         * <summary>Recursive method for computing the agent neighbors of the
         * specified agent.</summary>
         *
         * <param name="agent">The agent for which agent neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         * <param name="node">The current agent k-D tree node index.</param>
         */
        private void queryAgentTreeRecursive(Agent agent, ref JInt rangeSq, int node)
        {
            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {
                    agent.insertAgentNeighbor(agents_[i], ref rangeSq);
                }
            }
            else
            {
                JInt distSqLeft = RVOMath.sqr(ReduceMax(agentTree_[agentTree_[node].left_].minx , agent.position_.IntX) ) + RVOMath.sqr(ReduceMax(agent.position_.IntX ,agentTree_[agentTree_[node].left_].maxx)) + RVOMath.sqr(ReduceMax( agentTree_[agentTree_[node].left_].miny , agent.position_.IntY) ) + RVOMath.sqr(ReduceMax( agent.position_.IntY,agentTree_[agentTree_[node].left_].maxy) );

                JInt distSqRight = RVOMath.sqr(ReduceMax( agentTree_[agentTree_[node].right_].minx , agent.position_.IntX)) + RVOMath.sqr(ReduceMax( agent.position_.IntX , agentTree_[agentTree_[node].right_].maxx) ) + RVOMath.sqr(ReduceMax(agentTree_[agentTree_[node].right_].miny , agent.position_.IntY)) + RVOMath.sqr(ReduceMax(agent.position_.IntY,agentTree_[agentTree_[node].right_].maxy) );

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left_);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right_);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right_);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left_);
                        }
                    }
                }

            }
        }

        /**
         * <summary>Recursive method for computing the obstacle neighbors of the
         * specified agent.</summary>
         *
         * <param name="agent">The agent for which obstacle neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         * <param name="node">The current obstacle k-D node.</param>
         */
        private void queryObstacleTreeRecursive(Agent agent, JInt rangeSq, ObstacleTreeNode node)
        {
            if (node != null)
            {
                Obstacle obstacle1 = node.obstacle_;
                Obstacle obstacle2 = obstacle1.next_;

                JInt agentLeftOfLine = RVOMath.leftOf(obstacle1.point_, obstacle2.point_, agent.position_);

                queryObstacleTreeRecursive(agent, rangeSq, agentLeftOfLine >= 0 ? node.left_ : node.right_);
                if(RVOMath.absSq(obstacle2.point_ - obstacle1.point_) == 0)
                {
                    return;
                }

                JInt distSqLine = RVOMath.sqr(agentLeftOfLine) / RVOMath.absSq(obstacle2.point_ - obstacle1.point_);

                if (distSqLine < rangeSq)
                {
                    if (agentLeftOfLine < 0)
                    {
                        /*
                         * Try obstacle at this node only if agent is on right side of
                         * obstacle (and can see obstacle).
                         */
                        agent.insertObstacleNeighbor(node.obstacle_, rangeSq);
                    }

                    /* Try other side of line. */
                    queryObstacleTreeRecursive(agent, rangeSq, agentLeftOfLine >= 0 ? node.right_ : node.left_);
                }
            }
        }

        /**
         * <summary>Recursive method for querying the visibility between two
         * points within a specified radius.</summary>
         *
         * <returns>True if q1 and q2 are mutually visible within the radius;
         * false otherwise.</returns>
         *
         * <param name="q1">The first point between which visibility is to be
         * tested.</param>
         * <param name="q2">The second point between which visibility is to be
         * tested.</param>
         * <param name="radius">The radius within which visibility is to be
         * tested.</param>
         * <param name="node">The current obstacle k-D node.</param>
         */
        private bool queryVisibilityRecursive(Jint2 q1, Jint2 q2, JInt radius, ObstacleTreeNode node)
        {
            if (node == null)
            {
                return true;
            }

            Obstacle obstacle1 = node.obstacle_;
            Obstacle obstacle2 = obstacle1.next_;

            JInt q1LeftOfI = RVOMath.leftOf(obstacle1.point_, obstacle2.point_, q1);
            JInt q2LeftOfI = RVOMath.leftOf(obstacle1.point_, obstacle2.point_, q2);
            JInt LengthI = RVOMath.absSq(obstacle2.point_ - obstacle1.point_);
           // KInt invLengthI = 1.0f / RVOMath.absSq(obstacle2.point_ - obstacle1.point_);

            if (q1LeftOfI >= 0 && q2LeftOfI >= 0)
            {
                return queryVisibilityRecursive(q1, q2, radius, node.left_) && ((RVOMath.sqr(q1LeftOfI) / LengthI >= RVOMath.sqr(radius) && RVOMath.sqr(q2LeftOfI) / LengthI >= RVOMath.sqr(radius)) || queryVisibilityRecursive(q1, q2, radius, node.right_));
            }

            if (q1LeftOfI <= 0 && q2LeftOfI <= 0)
            {
                return queryVisibilityRecursive(q1, q2, radius, node.right_) && ((RVOMath.sqr(q1LeftOfI) / LengthI >= RVOMath.sqr(radius) && RVOMath.sqr(q2LeftOfI) / LengthI >= RVOMath.sqr(radius)) || queryVisibilityRecursive(q1, q2, radius, node.left_));
            }

            if (q1LeftOfI >= 0 && q2LeftOfI <= 0)
            {
                /* One can see through obstacle from left to right. */
                return queryVisibilityRecursive(q1, q2, radius, node.left_) && queryVisibilityRecursive(q1, q2, radius, node.right_);
            }

            JInt point1LeftOfQ = RVOMath.leftOf(q1, q2, obstacle1.point_);
            JInt point2LeftOfQ = RVOMath.leftOf(q1, q2, obstacle2.point_);
            JInt LengthQ = RVOMath.absSq(q2 - q1);
           // KInt invLengthQ = 1.0f / RVOMath.absSq(q2 - q1);

            return point1LeftOfQ * point2LeftOfQ >= 0 && RVOMath.sqr(point1LeftOfQ) / LengthQ > RVOMath.sqr(radius) && RVOMath.sqr(point2LeftOfQ) / LengthQ > RVOMath.sqr(radius) && queryVisibilityRecursive(q1, q2, radius, node.left_) && queryVisibilityRecursive(q1, q2, radius, node.right_);
        }
    }
}
