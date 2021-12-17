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
        
        void Calculate2Triangle(Func<int, int, int, int, int, int, bool> action, Action<int, int> action2)
        {
            for (int i = 0; i < indices.Length - 3; i += 3)
            for (int j = i + 3; j < indices.Length; j += 3)
            {
                if (action(indices[i + 0], indices[i + 1], indices[i + 2], indices[j + 0], indices[j + 1], indices[j + 2]))
                {
                    action2(i / 3, j / 3);
                }
            }
        }
    }
}