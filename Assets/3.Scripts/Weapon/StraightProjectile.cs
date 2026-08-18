using UnityEngine;

public class StraightProjectile : Projectile
{
    private Rigidbody projectileRigidbody;

    private Vector3 moveDirection;

    protected override void HandleTrigger(Collider collider)
    {
        CharacterDamageReceiver receiver = collider.GetComponentInParent<CharacterDamageReceiver>();

        if (receiver != null)
        {
            DamageInfo damageInfo = CreateDamageInfo(collider, moveDirection);

            receiver.ReceiveDamage(damageInfo);
            Release();
            return;
        }

        if (!collider.isTrigger)
        {
            Release();
        }
    }

    protected override bool TryShoot(Vector3 direction)
    {
        direction.y = 0f;

        if(direction.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        //���� ���ϱ�
        moveDirection = direction.normalized;

        //�� ���� ���ϱ�
        transform.forward = moveDirection;

        GetComponent<Rigidbody>().velocity = moveDirection * ProjectileData.Speed;

        return true;
    }
}
