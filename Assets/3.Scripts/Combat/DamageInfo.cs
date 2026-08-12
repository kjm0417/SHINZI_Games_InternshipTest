using UnityEngine;

public readonly struct DamageInfo
{
    public float Damage { get; }
    public GameObject Attacker { get; }
    public Vector3 HitPoint { get; }
    public Vector3 KnockbackDirection { get; }
    public float KnockbackPower { get; }

    public DamageInfo(float damage, GameObject attacker, Vector3 hitPoint, 
        Vector3 knockbackDirection, float knockbackPower)
    {
        Damage = Mathf.Max(0f, damage);
        Attacker = attacker;
        HitPoint = hitPoint;
        KnockbackDirection = knockbackDirection.sqrMagnitude > 0.001f
            ? knockbackDirection.normalized
            : Vector3.zero;
        KnockbackPower = Mathf.Max(0f, knockbackPower);
    }
}
