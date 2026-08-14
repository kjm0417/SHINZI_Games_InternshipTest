using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    SeekWeapon,
    Chase,
    Dead
}

public class AIBrain 
{
    public AIState CurrentState { get; private set; }
    public Transform CurrentTarget { get; private set; }

    public void Decide(Transform playerTarget, WeaponPickup weaponPickup, WeaponData currentWeapon)
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

        if (playerTarget != null)
        {
            CurrentState = AIState.Chase;
            CurrentTarget = playerTarget;
            return;
        }

        CurrentState = AIState.Idle;
        CurrentTarget = null;
    }

    public void SetDead()
    {
        CurrentState = AIState.Dead;
    }
}
