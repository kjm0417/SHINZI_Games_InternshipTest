using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealthSystem : MonoBehaviour
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float, float> HealthChanged; //현재 체력과 최대체력 넘겨주기
    public event Action Died;

    public void Initialize(float maxHealth)
    {
        if (maxHealth <= 0f) return;

        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;

        IsDead = false;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f) return;

        CurrentHealth = Mathf.Max(0,CurrentHealth-damage);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if(CurrentHealth<=0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        Died?.Invoke();

    }

    //테스트용
    [ContextMenu("Kill")]
    public void Kill()
    {
        TakeDamage(CurrentHealth);
    }
}
