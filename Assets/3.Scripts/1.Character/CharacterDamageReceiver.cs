using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDamageReceiver : MonoBehaviour
{
    //필요한 정보
    [SerializeField] private CharacterHealthSystem healthSystem;
    private IKnockbackReceiver knockbackReceiver;

    private void Awake()
    {
        if (healthSystem == null) healthSystem = GetComponent<CharacterHealthSystem>();
        if (knockbackReceiver == null) knockbackReceiver = GetComponent<IKnockbackReceiver>();
    }

    public void ReceiveDamage(DamageInfo info)
    {
        if (healthSystem.IsDead) return;

        healthSystem.TakeDamage(info.Damage);

        if (info.KnockbackPower > 0f)
        {
            knockbackReceiver.ApplyKnockback( info.KnockbackDirection, info.KnockbackPower);
        }

    }
}

