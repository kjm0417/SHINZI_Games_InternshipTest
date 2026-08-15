using UnityEngine;
using UnityEngine.AddressableAssets;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;

    private WeaponData weaponData;
    private GameObject owner;
    private Vector3 moveDirection;
    private bool initialized;
    private bool released;

    public void Initialize(WeaponData weaponData, GameObject owner, Vector3 direction)
    {
        if (weaponData == null || weaponData.ProjectileId == null || owner == null)
        {
            Release();
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            Release();
            return;
        }

        this.weaponData = weaponData;
        this.owner = owner;

        moveDirection = direction.normalized;
        initialized = true;

        transform.forward = moveDirection;

        rigidBody.velocity = moveDirection * weaponData.ProjectileId.Speed;

        Invoke(nameof(Release), weaponData.ProjectileId.LifeTime);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!initialized || released) return;

        //발사자 자신과 발사자가 가진 Collider는 무시
        if (collider.transform.root == owner.transform.root) return;

        CharacterDamageReceiver receiver = collider.GetComponentInParent<CharacterDamageReceiver>();

        if (receiver != null)
        {
            Vector3 hitPoint = collider.ClosestPoint(transform.position);

            DamageInfo damageInfo = new DamageInfo(
                weaponData.Damage,
                owner,
                hitPoint,
                moveDirection,
                weaponData.KnockbackPower);

            receiver.ReceiveDamage(damageInfo);
            Release();
            return;
        }

        //무기 Pickup 같은 다른 Trigger는 통과하고,
        //벽과 장애물 같은 일반 Collider에 맞으면 제거
        if (!collider.isTrigger)
        {
            Release();
        }
    }

    private void Release()
    {
        if (released) return;

        released = true;
        CancelInvoke();

        Addressables.ReleaseInstance(gameObject);
    }
}