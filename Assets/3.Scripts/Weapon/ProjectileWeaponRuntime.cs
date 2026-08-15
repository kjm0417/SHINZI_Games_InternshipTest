using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeaponRuntime : WeaponRuntime
{
    [SerializeField] private Transform spawnPoint;

    public override void ExecuteAttack()
    {
        if (IsAttacking) return;

        if (WeaponData == null || WeaponData.ProjectileId == null || Owner == null || spawnPoint == null)
        {
            return;
        }
    }

   
}
