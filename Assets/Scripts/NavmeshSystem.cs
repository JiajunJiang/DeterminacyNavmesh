using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class NavmeshSystem : Singleton<NavmeshSystem>
    {
        /// <summary>
        /// Same to Editor Precision
        /// </summary>
        public static int Precision = 100;

        public Point3D[] vertices;
        public int[] indices;
        public int[] lines;
        public List<Node> nodes;

        public void Init(byte[] bytes)
        {
            NavMeshFileInfo info = NavMeshFileInfo.Parser.ParseFrom(bytes);
            vertices = new Point3D[info.Vertices.Count];
            indices = new int[info.Indices.Count];
            lines = new int[info.Lines.Count];

            for (var index = 0; index < info.Vertices.Count; index++)
            {
                var vertex = info.Vertices[index];
                vertices[index] = new Point3D(vertex.X, vertex.Y, vertex.Z);
            }

            for (var index = 0; index < info.Indices.Count; index++)
            {
                var infoIndex = info.Indices[index];
                indices[index] = infoIndex;
            }

            for (var index = 0; index < info.Lines.Count; index++)
            {
                var line = info.Lines[index];
                lines[index] = line;
            }

            InitNodes();
        }

        private void InitNodes()
        {
            nodes = new List<Node>(vertices.Length);
            CalculateTriangle((p1, p2, p3) =>
            {
                var node = new Node(vertices[p1], vertices[p2], vertices[p3]) {index = nodes.Count};
                nodes.Add(node);
            });
            Calculate2Triangle((a, b, c, d, e, f) =>
            {
                var count = 0;
                if (a == d)
                {
                    count++;
                }

                if (a == e) count++;
                if (a == f) count++;
                if (b == d) count++;
                if (b == e) count++;
                if (b == f) count++;
                if (c == d) count++;
                if (c == e) count++;
                if (c == f) count++;
                return count >= 2;
            }, (i, j) =>
            {
                nodes[i].surrounds.Add(nodes[j]);
                nodes[j].surrounds.Add(nodes[i]);
            });
        }

        private void CalculateTriangle(Action<int, int, int> action)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                action(indices[i + 0], indices[i + 1], indices[i + 2]);
            }
        }

        private void Calculate2Triangle(Func<int, int, int, int, int, int, bool> action, Action<int, int> action2)
        {
            for (int i = 0; i < indices.Length - 3; i += 3)
            for (int j = i + 3; j < indices.Length; j += 3)
            {
                if (action(indices[i + 0], indices[i + 1], indices[i + 2], indices[j + 0], indices[j + 1],
                    indices[j + 2]))
                {
                    action2(i / 3, j / 3);
                }
            }
        }

        public List<Point3D> CalculatePath(Point3D from, Point3D to)
        {
            int startTriangleIndex = -1;
            int endTriangleIndex = -1;

            if (!IsPointInMesh(from, out startTriangleIndex))
            {
                Debug.LogError("Start Point is out of mesh");
                return null;
            }

            if (!IsPointInMesh(to, out endTriangleIndex))
            {
                Debug.LogError("End Point is out of mesh");
                return null;
            }

            var nodeFrom = nodes[startTriangleIndex / 3];
            var nodeTo = nodes[endTriangleIndex / 3];
            var nodePath = AStarPathSearch(nodeFrom, nodeTo);
            if (nodePath == null)
                return null;//if return null,it means some node not connect
            return OptimizePath(nodePath, from, to);
        }

        private List<Point3D> path = new List<Point3D>(64);

        private Point3D[] tempNodePoint3D = new Point3D[2];
        Point3D tempPoint1;
        Point3D tempPoint2;

        private List<Point3D> OptimizePath(List<Node> nodePath, Point3D from, Point3D to)
        {
            path.Clear();
            bool isNewEye = true;
            Point3D eye, p1, p2;
            Point3D n1, n2;
            eye = from;
            p1 = eye;
            p2 = eye;

            var triangleIndex1 = 0;
            var triangleIndex2 = 0;

            for (int i = 0; i < nodePath.Count; i++)
            {
                if (i == nodePath.Count - 1)
                {
                    tempNodePoint3D[0] = to;
                    tempNodePoint3D[1] = to;
                }
                else
                {
                    GetSharedPoints(nodePath[i].index, nodePath[i + 1].index, eye, out tempPoint1, out tempPoint2);
                    tempNodePoint3D[0] = tempPoint1;
                    tempNodePoint3D[1] = tempPoint2;
                }

                if (isNewEye)
                {
                    if (tempNodePoint3D[0].Equals(eye) || tempNodePoint3D[1].Equals(eye))
                        continue;

                    path.Add(eye);

                    p1 = tempNodePoint3D[0] - eye;
                    p2 = tempNodePoint3D[1] - eye;

                    if (Point2D.Cross_XZ(p1, p2) == 0)
                        continue;
                    isNewEye = false;
                    continue;
                }

                n2 = tempNodePoint3D[0] - eye;
                n1 = tempNodePoint3D[1] - eye;

                var p1N2 = Point2D.Cross_XZ(p1, n2);
                var n2P2 = Point2D.Cross_XZ(n2, p2);
                var p1N1 = Point2D.Cross_XZ(p1, n1);
                var n1P2 = Point2D.Cross_XZ(n1, p2);

                if (p1N2 >= 0 && n2P2 >= 0)
                {
                    p1 = n2;
                    triangleIndex1 = i;
                }

                if (p1N1 >= 0 && n1P2 >= 0)
                {
                    p2 = n1;
                    triangleIndex2 = i;
                }

                if (n2P2 < 0)
                {
                    eye = eye + p2;
                    i = triangleIndex2;
                    isNewEye = true;
                    continue;
                }

                if (p1N1 < 0)
                {
                    eye = eye + p1;
                    i = triangleIndex1;
                    isNewEye = true;
                    continue;
                }
            }

            path.Add(to);
            return path;
        }

        List<int> tempSharedPoints = new List<int>(3);

        void GetSharedPoints(int a, int b, Point3D eye, out Point3D point1, out Point3D point2)
        {
            tempSharedPoints.Clear();
            var a3 = a * 3;
            var b3 = b * 3;
            for (int i = a3; i < a3 + 3; i++)
            {
                for (int j = b3; j < b3 + 3; j++)
                {
                    if (indices[i] == indices[j])
                    {
                        tempSharedPoints.Add(indices[i]);
                        break;
                    }
                }
            }

            var e = indices[a3] ^ indices[a3 + 1] ^ indices[a3 + 2] ^ tempSharedPoints[0] ^ tempSharedPoints[1];
            eye = vertices[e];
            var p1 = vertices[tempSharedPoints[0]];
            var p2 = vertices[tempSharedPoints[1]];
            if (Point2D.Cross_XZ(p1 - eye, p2 - eye) < 0)
            {
                point1 = p2;
                point2 = p1;
            }
            else
            {
                point1 = p1;
                point2 = p2;
            }
        }

        public bool IsPointInMesh(Point3D p, out int triangleIndex)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (FixedMath.IsInTriangleXZ(vertices[indices[i + 0]], vertices[indices[i + 1]],
                    vertices[indices[i + 2]], p))
                {
                    triangleIndex = i;
                    return true;
                }
            }

            triangleIndex = -1;
            return false;
        }

        List<Node> openList = new List<Node>(64);
        List<Node> closeList = new List<Node>(64);
        public List<Node> nodePath = new List<Node>(64);

        private List<Node> AStarPathSearch(Node nodeFrom, Node nodeTo)
        {
            openList.Clear();
            closeList.Clear();
            nodeFrom.parent = null;
            openList.Add(nodeFrom);
            while (openList.Count > 0)
            {
                openList.Sort((n1, n2) => n1.F - n2.F);
                var node = openList[0];
                if (node == nodeTo)
                {
                    nodePath.Clear();
                    while (null != node.parent)
                    {
                        nodePath.Insert(0, node);
                        node = node.parent;
                    }

                    nodePath.Insert(0, nodeFrom);
                    return nodePath;
                }

                foreach (var ns in node.GetSurround())
                {
                    if (closeList.Contains(ns))
                        continue;
                    if (openList.Contains(ns))
                    {
                        var newDist = ns.G + ns.GetDistance(node);
                        if (node.G > newDist)
                        {
                            node.parent = ns;
                            node.G = newDist;
                        }

                        continue;
                    }

                    ns.G = node.G + ns.GetDistance(node);
                    ns.H = ns.GetDistance(nodeTo);
                    ns.parent = node;
                    openList.Add(ns);
                }

                openList.RemoveAt(0);
                closeList.Add(node);
            }

            return null;
        }
        
        public bool IsLineInsideMesh(Point3D p1, Point3D p2)
        {
            for (int i = 0; i < lines.Length; i += 2)
            {
                if (FixedMath.IsLineCross(vertices[lines[i + 0]].XZ, vertices[lines[i + 1]].XZ, p1.XZ, p2.XZ))
                {
                    return false;
                }
            }
            return true;
        }
        
        
        public bool IsLineInsideMesh(Point3D p1, Point3D p2,bool first,ref Point2D outP1,ref Point2D outP2)
        {
            outP1 = new Point2D();
            outP2 = new Point2D();

            bool result = false;
            
            for (int i = 0; i < lines.Length; i += 2)
            {
                if (FixedMath.IsLineCross(vertices[lines[i + 0]].XZ, vertices[lines[i + 1]].XZ, p1.XZ, p2.XZ))
                {
                    outP1 = vertices[lines[i + 0]].XZ;
                    outP2 = vertices[lines[i + 1]].XZ;
                    if (first) //return first line
                    {
                        result = true;
                        return true;
                    }
                }
            }

            return result;
        }
    }
}