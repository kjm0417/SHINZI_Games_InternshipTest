using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [SerializeField] private CharacterWeaponHolder weaponholder;
    [SerializeField] private CharacterWeaponView weaponView;

    private float cooldownRemaining;
    private float cooldownDuration;

    //ui전달용
    public float AttackCooldownNormalized => cooldownRemaining <= 0f || cooldownDuration <= 0f
        ? 0f : cooldownRemaining / cooldownDuration;

    //공격 할 수 있는지
    public bool CanAttack => weaponholder.HasWeapon && cooldownRemaining <= 0f 
        && weaponView.CurrentRuntime!=null 
        && !weaponView.CurrentRuntime.IsAttacking;

    //움직임 막기
    public bool BlocksMovement =>
    weaponView.CurrentRuntime != null &&
    weaponView.CurrentRuntime.BlocksMovement;

    //이벤트
    public event Action<WeaponData> AttackStarted;

    private void Awake()
    {
        if (weaponholder == null) weaponholder = GetComponentInChildren<CharacterWeaponHolder>();
        if (weaponView == null) weaponView = GetComponentInChildren<CharacterWeaponView>();
    }

    private void Update()
    {
        if (cooldownRemaining <= 0f) return;

        cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
    }
    public bool TryAttack()
    {
        //공격 가능한지 검사
        if (!CanAttack) return false;

        //현재 무기를 지역 변수에 저장
        WeaponRuntime runtime = weaponView.CurrentRuntime;

        //cooldownRemaining에 무기의 AttackCooldawn 적용
        cooldownDuration = runtime.WeaponData.AttackCooldown;
        cooldownRemaining = cooldownDuration;

        //공격 행동
        runtime.ExecuteAttack();

        AudioManager.Instance.PlayWeaponSound(weaponholder.CurrentWeapon.WeaponId);

        //공격 시작 알림
        AttackStarted?.Invoke(runtime.WeaponData);

        //성공 여부 반환
        return true;
    }
}
