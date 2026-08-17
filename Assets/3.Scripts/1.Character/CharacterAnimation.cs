using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{

    private CharacterHealthSystem healthSystem;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Vector3 previousPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<CharacterHealthSystem>();
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        healthSystem.Died += HandleDied;
    }

    private void OnDisable()
    {
        healthSystem.Died -= HandleDied;
    }

    private void HandleDied()
    {
        animator.SetFloat(SpeedHash, 0f);
        animator.SetTrigger(DieHash);
    }

    private void LateUpdate()
    {
        if (healthSystem.IsDead)
        {
            return;
        }


        Vector3 movement = transform.position - previousPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Time.deltaTime;

        animator.SetFloat(SpeedHash, speed);

        previousPosition = transform.position;
    }
}
