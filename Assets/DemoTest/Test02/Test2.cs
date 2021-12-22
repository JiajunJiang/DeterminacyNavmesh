using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public Vector3 Destination;

    private SuperNavAgent _agent;
    
    // Start is called before the first frame update
    void Start()
    {
        var file = Resources.Load("NavMesh/Test02", typeof(TextAsset)) as TextAsset;
        NavmeshSystem.Instance.Init(file.bytes);
        
        _agent = this.gameObject.GetComponent<SuperNavAgent>();
        _agent.SetLocation(new Point3D(transform.position * NavmeshSystem.Precision));

        
        _agent.SetDestination(new Point3D(Destination));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
