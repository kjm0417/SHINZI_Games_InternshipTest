using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    //참조 정보 
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIAim aim;
    [SerializeField] private CharacterHealthSystem healthSystem;
    [SerializeField] private CharacterCombat combat;
    [SerializeField] private CharacterWeaponHolder weaponHolder;

    //프로퍼티
    public CharacterHealthSystem HealthSystem => healthSystem;

    private AIBrain brain;
   
    private AIData aiData;
    private Transform target;
    private WeaponSpawner weaponSpawner;

    private bool isInitialized;

    //타이머
    private float decisionTimer; //목적지 갱신 타이머

    private void Awake()
    {
        if (weaponHolder == null) weaponHolder = GetComponent<CharacterWeaponHolder>();
        if (movement == null) movement = GetComponent<AIMovement>();
        if (aim == null) aim = GetComponent<AIAim>();
        if (healthSystem == null) healthSystem = GetComponent<CharacterHealthSystem>();
        if (combat == null) combat = GetComponent<CharacterCombat>();

    }

    public bool Initialize(AIData data, Transform playerTarget, WeaponSpawner spawner)
    {
        if (data == null) return false;
        if (playerTarget == null) return false;
        if (spawner == null) return false;

        aiData = data;
        target = playerTarget;
        weaponSpawner = spawner;

        brain = new AIBrain();

        movement.Initialize(data.Speed);
        healthSystem.Initialize(data.MaxHp);

        decisionTimer = 0f;
        isInitialized = true;

        return true;
    }

    private void OnEnable()
    {
        healthSystem.Died += HandleDied;
    }

    private void OnDisable()
    {
        healthSystem.Died -= HandleDied;
    }

    void Update()
    {
        if (!isInitialized) return;

        if (brain.CurrentState == AIState.Dead) return;

        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f) return;

        decisionTimer = aiData.BehaviorId.ReactionTime;

        WeaponData currentWeapon = weaponHolder.CurrentWeapon;
        WeaponPickup nearestWeapon = FindNearestWeapon();

        brain.Decide(target,transform.position, nearestWeapon, currentWeapon);
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
                if (brain.CurrentTarget == null)
                {
                    movement.Stop();
                    break;
                }

                aim.FaceTarget(brain.CurrentTarget.position);

                combat.TryAttack();

                if (combat.BlocksMovement)
                {
                    movement.Stop();
                }
                else
                {
                    movement.MoveTo(brain.CurrentTarget.position);
                }
                ;
                break;


        }
    }

    private void HandleDied()
    {
        brain.SetDead();
        movement.Stop();
    }

    private WeaponPickup FindNearestWeapon()
    {
        if (weaponSpawner == null) return null;

        IReadOnlyList<WeaponPickup> pickups = weaponSpawner.ActivePickups;

        WeaponPickup nearestPickup = null;

        //크기를 무한으로 설정
        float nearestSqrDistance = float.PositiveInfinity;

        for (int i = 0; i < pickups.Count; i++)
        {
            WeaponPickup pickup = pickups[i];

            if (pickup == null || !pickup.IsAvailable)
            {
                continue;
            }

            // 현재 들고 있는 무기와 같은 무기는 제외
            if (!weaponHolder.CanEquip(pickup.Data))
            {
                continue;
            }

            Vector3 offset = pickup.transform.position - transform.position;
            offset.y = 0f;

            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance >= nearestSqrDistance)
            {
                continue;
            }

            nearestSqrDistance = sqrDistance;
            nearestPickup = pickup;
        }

        return nearestPickup;

    }

    public void StopControl()
    {
        isInitialized = false;
        movement.Stop();
    }
}
