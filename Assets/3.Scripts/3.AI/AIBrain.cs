using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    SeekWeapon,
    Chase,
    Engage,
    Dead
}

public class AIBrain 
{
    public AIState CurrentState { get; private set; }
    public Transform CurrentTarget { get; private set; }

    public void Decide(Transform playerTarget, Vector3 selfPosition, WeaponPickup weaponTarget, WeaponData currentWeapon)
    {
        if (CurrentState == AIState.Dead) return;

        if (weaponTarget != null)
        {
            CurrentState = AIState.SeekWeapon;
            CurrentTarget = weaponTarget.transform;
            return;
        }

        if (playerTarget == null)
        {
            CurrentState = AIState.Idle;
            CurrentTarget = null;
            return;
        }

        CurrentTarget = playerTarget;

        if (currentWeapon == null)
        {
            CurrentState = AIState.Chase;
            return;
        }

        //방향과 거리 계산 y값은 탑뷰고 점프없으니 무시
        Vector3 offset = playerTarget.position - selfPosition;
        offset.y = 0f;

        float attackRange = Mathf.Max(0f, currentWeapon.Range);

        float sqrDistance = offset.sqrMagnitude;

        CurrentState = sqrDistance <= attackRange * attackRange ? AIState.Engage: AIState.Chase;

    }

    public bool TryDecideDodge(Vector3 attackerPosition, Vector3 selfPosition, float dodgeChance, out Vector3 dodgeDirection)
    {
        dodgeDirection = Vector3.zero;

        if (CurrentState == AIState.Dead)
        {
            return false;
        }

        if (Random.value >= dodgeChance)
        {
            return false;
        }

        Vector3 awayDirection = selfPosition - attackerPosition;
        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.001f)
        {
            return false;
        }

        Vector3 sideDirection = Vector3.Cross(Vector3.up, awayDirection.normalized);

        dodgeDirection = Random.value < 0.5f ? sideDirection  : -sideDirection;

        return true;
    }

    public void SetDead()
    {
        CurrentState = AIState.Dead;
        CurrentTarget = null;
    }
}
