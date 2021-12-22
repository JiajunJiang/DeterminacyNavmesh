using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class SuperNavAgent : MonoBehaviour
    {
        public SuperNavAgent()
        {
            Precision = NavmeshSystem.Precision;
        }

        public int Precision;
        public int Speed = 5000;
        public int FramePerSpeed = 25;
        private int length;
        private long totalLength = 0;

        public int Radius = 0;
        

        /// <summary>
        /// 逻辑层目的地位置
        /// </summary>
        public Point3D Destination { get; private set; }

        /// <summary>
        /// 逻辑层当前位置
        /// </summary>
        public Point3D Localtion { get; private set; }
        
        public Point3D Direction { get; private set; }

        public List<Point3D> path = new List<Point3D>(64);

        /// <summary>
        /// 是否在移动
        /// </summary>
        public bool IsMove
        {
            get { return totalLength != 0; }
        }

        /// <summary>
        /// 暂停移动 
        /// </summary>
        public bool Pause { get; set; }
        
        

        public void SetDestination(Point3D dest)
        {
            totalLength = 0;
            Destination = dest;
            length = 0;
            path.Clear();
            path.AddRange(NavmeshSystem.Instance.CalculatePath(Localtion, Destination));
            CalculateTotalLength();
        }

        private void CalculateTotalLength()
        {
            totalLength = 0;
            if (path?.Count >= 2)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    var secLen = (path[i] - path[i - 1]).Magnitude;
                    totalLength += secLen;
                }
            }
        }

        public void SetLocation(Point3D loca)
        {
            Localtion = loca;
            Destination = loca;
            length = 0;
            totalLength = 0;
            path = null;
        }

        public void Stop()
        {
            length = 0;
            totalLength = 0;
            path = null;
        }

        private void FixedUpdate()
        {
            length += Speed / FramePerSpeed;
            long len = 0;
            if (totalLength != 0 && length > totalLength)
            {
                Localtion = Destination;
                totalLength = 0;
                path = null;
            }

            if (path?.Count >= 2)
                for (int i = 1; i < path.Count; i++)
                {
                    var secLen = (path[i] - path[i - 1]).Magnitude;
                    if (len + secLen > length)
                    {
                        Localtion = path[i - 1] + (path[i] - path[i - 1]) * (length - len) / secLen;
                        break;
                    }

                    len += secLen;
                }

            var pos = Localtion.ToUnityVector3() / Precision;

            Direction = new Point3D((transform.position - pos) * Precision);

            //todo 贴地
            transform.position = pos;
        }
    }
}