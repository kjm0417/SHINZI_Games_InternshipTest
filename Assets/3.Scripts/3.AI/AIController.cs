using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    //참조 정보
    [SerializeField] private AIData aiData;
    [SerializeField] private Transform target;
    [SerializeField] private AIBrain brain;
    [SerializeField] private AIMovement movement;
    [SerializeField] private CharacterHealthSystem healthSystem;

    void Start()
    {
        healthSystem.Initialize(aiData.MaxHp);
    }

    void Update()
    {
        if (healthSystem.IsDead) return;

        brain.Tick(transform, target, aiData, Time.deltaTime);

        if (brain.WantsToDash)
        {
            movement.TryDash(brain.MoveDirection);
        }
          
        movement.Tick(brain.MoveDirection, Time.deltaTime);
    }
}
