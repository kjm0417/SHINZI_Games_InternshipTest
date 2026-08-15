using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour, IKnockbackReceiver
{
    [SerializeField] private AIData aiData;
    private NavMeshAgent agent;

    //³Ë¹é °ü·Ã
    [SerializeField] private float knockbackDecay = 20f;
    private Vector3 knockbackVelocity;

    public bool HasReachedDestination { get; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        UpdateKnockback(Time.deltaTime);
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

    public void ApplyKnockback(Vector3 direction, float power)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f || power <= 0f) return;

        knockbackVelocity = direction.normalized * power;
    }
    private void UpdateKnockback(float deltaTime)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            knockbackVelocity = Vector3.zero;
            return;
        }

        if (knockbackVelocity.sqrMagnitude < 0.001f) return;

        agent.Move(knockbackVelocity * deltaTime);

        knockbackVelocity = Vector3.MoveTowards( knockbackVelocity, Vector3.zero, knockbackDecay * deltaTime);

        
    }
}
