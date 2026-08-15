using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class Projectile : MonoBehaviour
{
    protected WeaponData WeaponData { get; private set; }
    protected GameObject Owner { get; private set; }
    protected ProjectileData ProjectileData => WeaponData.ProjectileId;

    private Transform ownerRoot; //최상위 오브젝트
    private bool initialized; //초기화 상태
    private bool released; //Addressable 빼기

    public void Initialize(WeaponData weaponData, GameObject owner, Vector3 direction)
    {
        if (weaponData == null || weaponData.ProjectileId == null || owner == null)
        {
            Release();
            return;
        }

        WeaponData = weaponData;
        Owner = owner;
        ownerRoot = owner.transform.root;

        if (!TryShoot(direction))
        {
            Release();
            return;
        }

        initialized = true;

        //projectile이 많아봐야 2~3개 여서 사용  -> 많아지면  코루틴이나 Update에서 관리
        Invoke(nameof(Release),Mathf.Max(0.01f, ProjectileData.LifeTime)); 

    }

    protected abstract bool TryShoot(Vector3 direction); //원거리 공격 시도
    protected abstract void HandleTrigger(Collider collider); //충돌 관련 원거리 무기마다 다름

    private void OnTriggerEnter(Collider collider)
    {
        if (!initialized || released) return;

        if (ownerRoot != null && collider.transform.root == ownerRoot)
        {
            return;
        }

        HandleTrigger(collider);
    }

    protected DamageInfo CreateDamageInfo( Collider collider, Vector3 knockbackDirection)
    {
        return new DamageInfo(
            WeaponData.Damage,
            Owner,
            collider.ClosestPoint(transform.position),
            knockbackDirection,
            WeaponData.KnockbackPower);
    }

    protected void Release()
    {
        if (released) return;

        released = true;
        CancelInvoke();

        Addressables.ReleaseInstance(gameObject);
    }


}