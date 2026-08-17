using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private Transform visualSocket;

   
    private GameObject currentVisual;
    private int loadVersion;
    private WeaponData weaponData;

    public WeaponData Data => weaponData;

    public bool IsAvailable => isActiveAndEnabled && weaponData != null;
    
    //데이터를 넣어줘야함
    public event Action<WeaponPickup> PickedUp;

    private void OnDisable()
    {
        //완료되지 않은 비동기 요청을 무효화
        ++loadVersion;

        ReleaseVisual();
    }


    //초기화 시키면서 addresable을 이용해서 외형 로드
    public bool Initialize(WeaponData data)
    {
        if (data == null) return false;
        if (visualSocket == null) return false;

        if(string.IsNullOrWhiteSpace(data.PrefabAddressable))
        {
            return false;
        }

        weaponData = data;
        ++loadVersion;
        int requestedVersion = loadVersion;

        ReleaseVisual();

        Addressables.InstantiateAsync(data.PrefabAddressable, visualSocket, false).Completed += handle =>
        {
            if(handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                return;
            }

            //WeaponPickup이 null이거나 로드 중인데 제거되거나 획득 되었을 경우 
            if (this == null || requestedVersion != loadVersion ||!gameObject.activeInHierarchy)
            {
                Addressables.ReleaseInstance(handle.Result);
                return;
            }

            currentVisual = handle.Result;
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
            //currentVisual.transform.localScale = Vector3.one; //테스트해보고 고민
        };

        return true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        CharacterWeaponHolder holder = collider.GetComponentInParent<CharacterWeaponHolder>();

        if (holder == null) return;

        if (!holder.TryEquip(weaponData)) return;

        gameObject.SetActive(false);
        PickedUp?.Invoke(this);
    }

    //외형 해제
    private void ReleaseVisual()
    {
        if (currentVisual == null) return;

        Addressables.ReleaseInstance(currentVisual);
        currentVisual = null;
    }
}
