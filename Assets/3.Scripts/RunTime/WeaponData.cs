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
    [SerializeField] private ProjectileData projectileId;

    [Tooltip("주소값")]
    [SerializeField] private string prefabAddressable;
    [SerializeField] private string soundAddressable;

    //프로퍼티 읽기 전용
    public string WeaponId => weaponId;
    public WeaponType WeaponType => weaponType;
    public string WeaponName => weaponName;
    public float Damage => damage;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;
    public float Range => range;
    public int KnockbackPower => knockbackPower;
    public ProjectileData ProjectileId => projectileId;
    public string PrefabAddressable => prefabAddressable;
    public string SoundAddressable => soundAddressable;

}

public enum WeaponType
{
    Melee,
    Range,
    Throw
}