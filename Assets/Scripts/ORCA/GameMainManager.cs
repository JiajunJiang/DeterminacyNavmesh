using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using FixedMath;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using Random = System.Random;

public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;

    public bool draw;

    [HideInInspector] public Vector2 mousePosition;

    private Plane m_hPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    public int cnt=1;

    // Use this for initialization
    void Start()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 1.5f, 2.0f, Jint2.zero);



        for(int i =0; i < cnt;++i)
        {
            CreatAgent();
        }

        Simulator.Instance.SetNumWorkers(1);
    }


    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (m_hPlane.Raycast(mouseRay, out rayDistance))
            position = mouseRay.GetPoint(rayDistance);

        mousePosition.x = position.x;
        mousePosition.y = position.z;
    }

    void DeleteAgent()
    {
        int agentNo = Simulator.Instance.queryNearAgent((Jint2)mousePosition, 1.5f);
        if (agentNo == -1 || !m_agentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        m_agentMap.Remove(agentNo);
    }

    public float neighborDist =15;
    public int maxNeighbors=10;
    public float timeHorizon =5;
    public float timeHorizonObst =5;
    public float radius = 2;
    public float maxSpeed =2;
    public Jint2 velocity;

    private int aaa = 1;
    
    void CreatAgent()
    {
        aaa++;
        int sid = Simulator.Instance.addAgent((Jint2)mousePosition, neighborDist,maxNeighbors,timeHorizon ,timeHorizonObst,aaa % 8,maxSpeed,velocity);
        if (sid >= 0)
        {
            GameObject go = Instantiate(agentPrefab);
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateMousePosition();
        if (Input.GetMouseButtonUp(0))
        {
            if (Input.GetKey(KeyCode.Delete))
            {
                DeleteAgent();
            }
            else
            {
                CreatAgent();
            }
        }

        Simulator.Instance.doStep();
        
    }

    void OnDrawGizmos()
    {
        if (draw)
            Simulator.Instance.DrawObstacles();
    }
}