using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//추상 선택 이유 : 공통 상태와 초기화 구현은 기반 클래스에서 공유하고,
//무기별 공격 실행만 파생 클래스에서 구현하기 위해 추상 클래스를 사용
public abstract class WeaponRuntime : MonoBehaviour
{
    public WeaponData WeaponData { get; private set; }
    public GameObject Owner { get; private set; }
    public bool IsAttacking { get; protected set; }

    //원거리 공격일 때만 이동이 멈춤 
    public bool BlocksMovement => IsAttacking && WeaponData != null && WeaponData.WeaponType == WeaponType.Range;
    //초기화
    public void Initialize(WeaponData weaponData, GameObject owner)
    {
        WeaponData = weaponData;
        Owner = owner;
        IsAttacking = false;

        OnInitialized();
    }

    protected virtual void OnInitialized()
    {

    }

    //각자 공격 
    public abstract void ExecuteAttack();
}
