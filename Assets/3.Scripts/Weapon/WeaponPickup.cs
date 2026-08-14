using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    public bool IsAvailable => isActiveAndEnabled && weaponData != null;

    public WeaponData Data => weaponData;

    private void OnTriggerEnter(Collider collider)
    {
        CharacterWeaponHolder holder = collider.GetComponentInChildren<CharacterWeaponHolder>();

        if (holder == null) return;

        if (!holder.TryEquip(weaponData)) return;

        gameObject.SetActive(false);
    }
}
