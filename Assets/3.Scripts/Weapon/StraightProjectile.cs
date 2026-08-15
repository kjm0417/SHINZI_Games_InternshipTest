using UnityEngine;

public class StraightProjectile : Projectile
{
    [SerializeField] private Rigidbody rigidbody;

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

        //방향 정하기
        moveDirection = direction.normalized;

        //앞 방향 정하기
        transform.forward = moveDirection;

        rigidbody.velocity = moveDirection * ProjectileData.Speed;

        return true;
    }
}
