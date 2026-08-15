using UnityEngine;

[CreateAssetMenu(fileName ="NewWeapon",menuName ="WeaponData/Weapon")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponId;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string weaponName;
    [SerializeField] private float damage;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackDuration;
    [SerializeField] private float range;
    [SerializeField] private int knockbackPower;

    [Tooltip("주소값")]
    [SerializeField] private string prefabAddressable;
    [SerializeField] private string iconAddressable;

    //프로퍼티 읽기 전용
    public string WeaponId => weaponId;
    public WeaponType WeaponType => weaponType;
    public string WeaponName => weaponName;
    public float Damage => damage;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;
    public float Range => range;

    public int KnockbackPower => knockbackPower;
    public string PrefabAddressable => prefabAddressable;
    public string IconAddressable => iconAddressable;

}

public enum WeaponType
{
    Melee,
    Range,
    Throw
}