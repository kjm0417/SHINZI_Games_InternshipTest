using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour
{
    [SerializeField] private AIData aiData;
    private CharacterController characterController;
    private NavMeshAgent navMeshAgent;
    
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    public void Tick(Vector3 moveDirection, float deltaTime)
    {

    }

    public bool TryDash(Vector3 direction)
    {
        return false;
    }

    public void ApplyKnockback(Vector3 direction, float power)
    {

    }
}
