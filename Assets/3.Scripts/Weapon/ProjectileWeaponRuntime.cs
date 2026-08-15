using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ProjectileWeaponRuntime : WeaponRuntime
{
    [SerializeField] private Transform spawnPoint;

    private Coroutine attackCoroutine;

    private void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        IsAttacking = false;
    }

    public override void ExecuteAttack()
    {
        if (IsAttacking) return;

        if (WeaponData == null || WeaponData.ProjectileId == null || Owner == null || spawnPoint == null)
        {
            return;
        }

        //addresable 주소 불러오기
        string projectileAddress = WeaponData.ProjectileId.PrefabAddressable;

        if (string.IsNullOrWhiteSpace(projectileAddress))
        {
            return;
        }

        IsAttacking = true;

        float attackDuration = WeaponData.AttackDuration;
        attackCoroutine =
            StartCoroutine(AttackRoutine(attackDuration));

        //공격 요청 순간의 값 보관
        WeaponData attackData = WeaponData;
        GameObject attacker = Owner;
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;
        Vector3 shootDirection = spawnPoint.forward;

        Addressables.InstantiateAsync(projectileAddress, spawnPosition, spawnRotation).Completed += handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"투사체 생성 실패: {projectileAddress}");

                Addressables.Release(handle);
                return;
            }

            Projectile projectile = handle.Result.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError($"{projectileAddress}의 루트에 Projectile 상속 컴포넌트가 없음");

                Addressables.ReleaseInstance(handle.Result);
                return;
            }

            projectile.Initialize( attackData, attacker, shootDirection);
        };

    }

    private IEnumerator AttackRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        IsAttacking = false;
        attackCoroutine = null;
    }
}
