using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

//손에 어떤 무기를 가지고 있는지
public class CharacterWeaponView : MonoBehaviour
{
    [SerializeField] private CharacterWeaponHolder weaponHolder;
    [SerializeField] private Transform weaponSocket;

    private GameObject currentWeaponObject; //현재 무기
    private int loadVersion; //가장 최근 무기 로드 요청을 식별

    public WeaponRuntime CurrentRuntime { get; private set; }

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

        //변경 요청이 들어올 때 각 요청에 번호 적용
        int requestedVersion = ++loadVersion; 

        ReleaseCurrentWeapon();

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

            //이미 다른 무기를 요청했으므로 이번 결과 해제
            if (requestedVersion != loadVersion)
            {
                Addressables.ReleaseInstance(handle.Result);
                return;
            }

            //프로퍼티에 바로하지 않고 검증이 완료된 후 넣기
            WeaponRuntime runtime = handle.Result.GetComponent<WeaponRuntime>();

            if (runtime == null)
            {
                //Debug.LogError($"런타임 없음 : {}");
                Addressables.ReleaseInstance(handle);
                return;
            }

            runtime.Initialize(weaponData, weaponHolder.gameObject);

            CurrentRuntime = runtime;
            currentWeaponObject = handle.Result;
            currentWeaponObject.transform.localPosition = Vector3.zero;
            currentWeaponObject.transform.localRotation = Quaternion.identity;
        };

        
    }

    private void ReleaseCurrentWeapon()
    {
        CurrentRuntime = null;

        if (currentWeaponObject == null) return;

        Addressables.ReleaseInstance(currentWeaponObject);
        currentWeaponObject = null;
    }

    //ai나 플레이어가 죽었을 때
    private void OnDestroy()
    {
        ++loadVersion;
        ReleaseCurrentWeapon();
    }
}
