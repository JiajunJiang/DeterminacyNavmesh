using System.Collections.Generic;

namespace DefaultNamespace
{
    public class Node
    {
        public Node parent;
        public int index;

        /// <summary>
        /// 当前点与起始点的距离
        /// </summary>
        public int G { get; set; }

        /// <summary>
        /// 当前点与目标点的距离
        /// </summary>
        public int H { get; set; }

        public int F => G + H;

        private Point3D P1;
        private Point3D P2;
        private Point3D P3;

        private Point3D Center;

        public readonly List<Node> surrounds = new List<Node>();

        public Node(Point3D p1, Point3D p2, Point3D p3)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;

            this.Center = (p1 + p2 + p3) / 3;
        }

        public int GetDistance(Node n)
        {
            return (int) (Center - n.Center).Magnitude;
        }

        public List<Node> GetSurround()
        {
            return surrounds;
        }
    }
}