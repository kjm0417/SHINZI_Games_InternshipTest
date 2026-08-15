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

    public void Decide(Transform playerTarget, Vector3 selfPosition, WeaponPickup weaponPickup, WeaponData currentWeapon)
    {
        if (CurrentState == AIState.Dead) return;

        bool canChangeWeapon = weaponPickup != null  && weaponPickup.IsAvailable &&
            (currentWeapon == null || currentWeapon.WeaponId != weaponPickup.Data.WeaponId);

        if (canChangeWeapon)
        {
            CurrentState = AIState.SeekWeapon;
            CurrentTarget = weaponPickup.transform;
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

    public void SetDead()
    {
        CurrentState = AIState.Dead;
    }
}
