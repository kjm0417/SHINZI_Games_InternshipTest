using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    //참조 정보
    [SerializeField] private AIData aiData;
    [SerializeField] private Transform target;
    [SerializeField] private AIMovement movement;
    [SerializeField] private CharacterHealthSystem healthSystem;
    [SerializeField] private WeaponPickup weaponPickup;
    [SerializeField] private CharacterCombat combat;
    private AIBrain brain;
    private CharacterWeaponHolder weaponHolder;

    //타이머
    private float decisionTimer; //목적지 갱신 타이머

    private void Awake()
    {
        brain = new AIBrain();
        weaponHolder = GetComponent<CharacterWeaponHolder>();
        if (combat == null) combat = GetComponent<CharacterCombat>();
        if (target ==null) target = GameObject.FindGameObjectWithTag("Player").transform;
        
    }

    private void OnEnable()
    {
        healthSystem.Died += HandleDied;
    }

    private void OnDisable()
    {
        healthSystem.Died -= HandleDied;
    }


    void Start()
    {
        healthSystem.Initialize(aiData.MaxHp);
    }

    void Update()
    {
        if (brain.CurrentState == AIState.Dead) return;

        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f) return;

        decisionTimer = aiData.BehaviorId.ReactionTime;
        brain.Decide(target,transform.position, weaponPickup, weaponHolder.CurrentWeapon);
        ExecuteState();
    }

    //상태에 따라 행동
    private void ExecuteState()
    {
        switch (brain.CurrentState)
        {
            case AIState.Idle:
                movement.Stop();
                break;

            case AIState.SeekWeapon:
            case AIState.Chase:
                if (brain.CurrentTarget != null)
                {
                    movement.MoveTo(brain.CurrentTarget.position);
                }
                break;
            case AIState.Engage:
                movement.Stop(); //테스트용 Stop
                combat.TryAttack();
                break;


        }
    }

    private void HandleDied()
    {
        brain.SetDead();
        movement.Stop();
    }
}
