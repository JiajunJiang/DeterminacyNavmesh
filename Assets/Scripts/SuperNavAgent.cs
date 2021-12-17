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
        public int Speed;
        public int FramePerSpeed;
        private int length;
        
        /// <summary>
        /// 逻辑层目的地位置
        /// </summary>
        public Point3D destination { get; private set; }
        
        /// <summary>
        /// 逻辑层当前位置
        /// </summary>
        public Point3D localtion { get; private set; }

        public List<Point3D> path;

        public void SetDestination(Point3D dest)
        {
            destination = dest;
            length = 0;
            path = NavmeshSystem.Instance.CalculatePath(localtion, destination);
        }
        
        public void SetLocation(Point3D loca)
        {
            localtion = loca;
            destination = loca;
            length = 0;
            path = null;
        }
        
        private void FixedUpdate()
        {
            length += Speed / FramePerSpeed;
            long len = 0;
            if (path?.Count >= 2)
                for (int i = 1; i < path.Count; i++)
                {
                    var secLen = (path[i] - path[i - 1]).Magnitude;
                    if (len + secLen > length)
                    {
                        localtion = path[i - 1] + (path[i] - path[i - 1]) * (length - len) / secLen;
                        break;
                    }
                    len += secLen;
                }
            
            var pos = localtion.ToUnityVector3() / NavmeshSystem.Precision;

            //todo 贴地
            transform.position = pos;
        }
    }
}