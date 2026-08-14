using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour
{
    [SerializeField] private AIData aiData;
    private NavMeshAgent agent;

    public bool HasReachedDestination { get; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    public void Initialize(float speed, float stoppingDistance)
    {

    }

    public bool SetDestination(Vector3 destination)
    {
        if(agent.enabled && agent.isOnNavMesh)
        {

        }
        return true;
    }

    public void MoveTo(Vector3 destionation)
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(destionation);
    }

    public void Stop()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
    }

    
}
