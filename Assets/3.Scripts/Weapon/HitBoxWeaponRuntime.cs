using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxWeaponRuntime : WeaponRuntime
{
    [SerializeField] private Collider hitbox;


    private Coroutine attackCoroutine; //코루틴
    private Animator animator; //애니메이터
    private static readonly int AttackHash = Animator.StringToHash("Attack"); //애니메이션 해시

    //피격 기록
    private readonly HashSet<CharacterDamageReceiver> hitTargets = new();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        hitbox.enabled = false;
    }

    private void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
            
        hitTargets.Clear();
        IsAttacking = false;
    }

    public override void ExecuteAttack()
    {
        if (IsAttacking) return;

        animator.SetTrigger(AttackHash);
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        hitTargets.Clear();
        hitbox.enabled = true;

        yield return new WaitForSeconds(WeaponData.AttackDuration);

        hitbox.enabled = false;
        IsAttacking = false;
        attackCoroutine = null;
    }

    private void OnTriggerEnter(Collider collider)
    {
        CharacterDamageReceiver receiver = collider.GetComponentInParent<CharacterDamageReceiver>();

        if (receiver == null || receiver.gameObject == Owner) return;

        if (!hitTargets.Add(receiver)) return;

        Vector3 knockbackDirection = receiver.transform.position - Owner.transform.position;
        Vector3 hitPoint =collider.ClosestPoint(hitbox.transform.position);

        DamageInfo damageinfo = new DamageInfo(
            WeaponData.Damage, 
            Owner,
            hitPoint, 
            knockbackDirection, 
            WeaponData.KnockbackPower);
    
        receiver.ReceiveDamage(damageinfo);
    }
}
