using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

//손에 어떤 무기를 가지고 있는지
public class CharacterWeaponView : MonoBehaviour
{
    [SerializeField] private CharacterWeaponHolder weaponHolder;
    [SerializeField] private Transform weaponSocket;

    private GameObject currentWeaponObject; //현재 무기

    void Awake()
    {
        if (weaponHolder == null) weaponHolder = GetComponent<CharacterWeaponHolder>();
    }

    private void OnEnable()
    {
        weaponHolder.WeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        weaponHolder.WeaponChanged -= HandleWeaponChanged;
    }


    private void HandleWeaponChanged(WeaponData weaponData)
    {
        if (weaponData == null) return;

        //IsNullOrWhiteSpace 이유
        if (string.IsNullOrWhiteSpace(weaponData.PrefabAddressable)) return;

        //InstantiateAsync -> 키 : 주소, 생성 위치 : weaponSocket,
        //false -> instantiateInWorldSpace : 기존 월드 좌표 유지 부모에 붙임, 4번째 인자 : trackHandle 기본값 true
        //handle 정보 : Status : 성공, 실패  Result : 성공한 결과 GameObject, OperationException 실패 원인, IsDone 완료 여부
        Addressables.InstantiateAsync(weaponData.PrefabAddressable, weaponSocket, false).Completed += handle =>
        {
            if(handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"무기 로드 실패: {weaponData.PrefabAddressable}");

                Addressables.Release(handle);
                return;
            }

            currentWeaponObject = handle.Result;
            currentWeaponObject.transform.localPosition = weaponSocket.transform.localPosition;
            currentWeaponObject.transform.localRotation = Quaternion.identity;
        };
    }
}
