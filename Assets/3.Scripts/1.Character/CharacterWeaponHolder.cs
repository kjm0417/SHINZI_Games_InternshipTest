using System;
using UnityEngine;

//실제로 어떤 무기를 보유하고있는지 관리
public class CharacterWeaponHolder : MonoBehaviour
{
    public WeaponData CurrentWeapon { get; private set; }
    public bool HasWeapon => CurrentWeapon != null;

    public event Action<WeaponData> WeaponChanged; //무기 변경되었는지

    public bool TryEquip(WeaponData newWeapon)
    {
        if (newWeapon == null) return false;

        if (CurrentWeapon != null && CurrentWeapon.WeaponId == newWeapon.WeaponId) return false;

        CurrentWeapon = newWeapon;
        WeaponChanged?.Invoke(CurrentWeapon);
        return true;
    }


}
