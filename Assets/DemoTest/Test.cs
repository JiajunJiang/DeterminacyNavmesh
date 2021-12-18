using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using DefaultNamespace;
using UnityEngine;

public class Test : MonoBehaviour
{
    public SuperNavAgent agent;
    public bool showEdgesLine = false;

    private void Start()
    {
        var file = Resources.Load("NavMesh/SampleScene", typeof(TextAsset)) as TextAsset;
        NavmeshSystem.Instance.Init(file.bytes);
        agent.SetLocation(new Point3D(transform.position * NavmeshSystem.Precision));
        LineSegment();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(new Point3D(hit.point * NavmeshSystem.Precision));
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                agent.SetLocation(new Point3D(hit.point * NavmeshSystem.Precision));
            }
        }

        //测试闪现
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hitPoint = new Point3D(hit.point * NavmeshSystem.Precision);
                if (NavmeshSystem.Instance.IsPointInMesh(hitPoint,out var index))
                {
                    agent.SetLocation(hitPoint);
                }
                else
                {
                    if (NavmeshSystem.Instance.IsLineInsideMesh(agent.Localtion,
                        hitPoint,true, ref crossP1, ref crossP2))
                    {
                        var intrPos = new Point2D();
                        SegmentsInterPoint(agent.Localtion.XZ, hitPoint.XZ, crossP1, crossP2, ref intrPos);
                        Debug.Log(intrPos.ToString());
                        agent.SetLocation(new Point3D(intrPos));
                    }
                }
            }
        }
    }
    
    public static bool SegmentsInterPoint(Point2D a, Point2D b, Point2D c, Point2D d, ref Point2D IntrPos)
    {
        //v1×v2=x1y2-y1x2 
        Point2D ab = b - a;
        Point2D ac = c - a;
        long abXac = Point2D.Cross(ab,ac);

        Point2D ad = d - a;
        long abXad = Point2D.Cross(ab, ad);

        if (abXac * abXad >= 0)
        {
            return false;
        }

        Point2D cd = d - c;
        Point2D ca = a - c;
        Point2D cb = b - c;

        long cdXca = Point2D.Cross(cd, ca);
        long cdXcb = Point2D.Cross(cd, cb);
        if (cdXca * cdXcb >= 0)
        {
            return false;
        }
        //计算交点坐标  
        long dx = Point2D.Cross(a -c, d -c) * (b.x - a.x) / Point2D.Cross (d-c,b-a);
        long dy = Point2D.Cross(a -c, d -c) * (b.y - a.y) / Point2D.Cross (d-c,b-a);

        IntrPos = new Point2D(a.x + dx, a.y + dy);
        return true;
    }

    /// <summary>
    /// 测试外围线段
    /// </summary>
    public void LineSegment()
    {
        var lineCount = new Dictionary<LineSegment, int>();
        CalculateTriangle((a, b, c) =>
        {
            var ab = new LineSegment(a, b);
            var bc = new LineSegment(b, c);
            var ca = new LineSegment(a, c);
            if (!lineCount.ContainsKey(ab)) lineCount[ab] = 0;
            if (!lineCount.ContainsKey(bc)) lineCount[bc] = 0;
            if (!lineCount.ContainsKey(ca)) lineCount[ca] = 0;
            lineCount[ab] += 1;
            lineCount[bc] += 1;
            lineCount[ca] += 1;
        });

        lines = new List<LineSegment>();
        foreach (var item in lineCount)
        {
            if (item.Value == 1)
            {
                lines.Add(item.Key);
            }
        }
    }

    private List<LineSegment> lines;

    static void CalculateTriangle(Action<int, int, int> action)
    {
        for (int i = 0; i < NavmeshSystem.Instance.indices.Length; i += 3)
        {
            action(NavmeshSystem.Instance.indices[i + 0], NavmeshSystem.Instance.indices[i + 1],
                NavmeshSystem.Instance.indices[i + 2]);
        }
    }

    private Point2D crossP1;
    private Point2D crossP2;

    public void OnDrawGizmos()
    {
        if (Application.isPlaying && showEdgesLine)
        {
            // Gizmos.color = Color.white;
            // foreach (var line in lines)
            // {
            //     Gizmos.DrawLine(NavmeshSystem.Instance.vertices[line.p1].ToUnityVector3() / NavmeshSystem.Precision,
            //         NavmeshSystem.Instance.vertices[line.p2].ToUnityVector3() / NavmeshSystem.Precision);
            // }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(crossP1.x / NavmeshSystem.Precision, 0, crossP1.y / NavmeshSystem.Precision),
                new Vector3(crossP2.x / NavmeshSystem.Precision, 0, crossP2.y / NavmeshSystem.Precision));
        }
    }
}