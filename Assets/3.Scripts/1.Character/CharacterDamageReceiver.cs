using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDamageReceiver : MonoBehaviour
{
    //필요한 정보
    [SerializeField] private CharacterHealthSystem healthSystem;
    [SerializeField] private PlayerMovement movement;

    private void Awake()
    {
        if (healthSystem == null) healthSystem = GetComponent<CharacterHealthSystem>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
    }

    public void ReceiveDamage(DamageInfo info)
    {
        if (healthSystem.IsDead) return;

        healthSystem.TakeDamage(info.Damage);

        if (info.KnockbackPower > 0f)
        {
            movement.ApplyKnockback(info.KnockbackDirection,info.KnockbackPower);
        }

    }
}

