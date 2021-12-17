using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Test : MonoBehaviour
{
    public SuperNavAgent agent;

    private void Start()
    {
        var file = Resources.Load("NavMesh/SampleScene", typeof(TextAsset)) as TextAsset;
        NavmeshSystem.Instance.Init(file.bytes);
        agent.SetLocation(new Point3D(transform.position * NavmeshSystem.Precision));
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
    }
}
