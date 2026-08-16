using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponSpawner : MonoBehaviour
{
    //weaponPickup 주소 값으로 가져오기
    [Header("PickUp")]
    [SerializeField] private AssetReferenceGameObject weaponPickupPrefab;

    [Header("스폰 위치")]
    [SerializeField] private Transform[] spawnPoint;

    //WeaponPickup List 
    private readonly List<WeaponPickup> activePickups = new();
    public IReadOnlyList<WeaponPickup> ActivePickups => activePickups;

    //현재 사용 중이거나 비동기 생성을 예약한 스폰 위치
    private readonly HashSet<Transform> occupiedSpawnPoints = new();

    //Spanwer 시간
    private float spawnTimer;
    private bool isRunning;

    private MatchData currentMatch;

    //매치에 따라 초기화
    private int spawnVersion;

    public void OnDisable()
    {
        StopAndClear();
    }

    private void Update()
    {
        if (!isRunning || currentMatch == null) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f) return;

        if (TrySpawnWeapon())
        {
            spawnTimer = currentMatch.ItemDropCoolTime;
        }
    }

    public bool Initialize(MatchData matchData)
    {
        if (matchData == null) return false;

        if (matchData.DropId == null) return false;

        if (weaponPickupPrefab == null || !weaponPickupPrefab.RuntimeKeyIsValid())
        {
            return false;
        }

        if (spawnPoint == null || spawnPoint.Length == 0)
        {
            return false;
        }

        StopAndClear();

        currentMatch = matchData;

        //첫 무기는 매치 시작 직후 생성
        spawnTimer = 0f;
        isRunning = true;

        return true;
    }


    //무기 가중치 값을 더해서 가중치에 맞게 선택해서 넘겨주기
    private WeaponData SelectWeightedWeapon()
    {
        IReadOnlyList<DropEntry> entries = currentMatch.DropId.Entries;

        if (entries == null || entries.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            DropEntry entry = entries[i];

            if (entry == null || entry.WeaponId == null || entry.DropRate <= 0)
            {
                continue;
            }

            totalWeight += entry.DropRate;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int randomWeight = Random.Range(0, totalWeight);

        int accumulatedWeight = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            DropEntry entry = entries[i];

            if (entry == null || entry.WeaponId == null || entry.DropRate <= 0)
            {
                continue;
            }

            accumulatedWeight += entry.DropRate;

            if (randomWeight < accumulatedWeight)
            {
                return entry.WeaponId;
            }
        }

        return null;
    }

    private void HandlePickedUp(WeaponPickup pickup)
    {
        pickup.PickedUp -= HandlePickedUp;

        activePickups.Remove(pickup);

        Transform usedPoint = pickup.transform.parent;

        if(usedPoint != null)
        {
            occupiedSpawnPoints.Remove(usedPoint);
        }

        Addressables.ReleaseInstance(pickup.gameObject);
    }

    //bool과 out 같이 사용한 이유 : 모두 사용중이면 없다고 알려줘야하기때문에 같이 사용
    private bool TrySelectSpawnPoint(out Transform selectedPoint)
    {
        selectedPoint = null;

        int availableCount = 0;

        //사용가능한 위치 계산
        for(int i =0; i<spawnPoint.Length;i++)
        {
            Transform point = spawnPoint[i];

            if(point ==null || occupiedSpawnPoints.Contains(point))
            {
                continue;
            }

            availableCount++;
        }

        //사용가능한 스폰너가 없으면 실패
        if(availableCount ==0)
        {
            return false;
        }

        //사용가능한 자리중 몇번째를 할지 선택 : 비어 있는것 중에서도 랜덤으로 스폰시키기위해
        int selectedIndex = Random.Range(0, availableCount);

        for(int i = 0;i< spawnPoint.Length;i++)
        {
            Transform point = spawnPoint[i];

            if (point == null || occupiedSpawnPoints.Contains(point))
            {
                continue;
            }

            if (selectedIndex == 0)
            {
                selectedPoint = point;
                return true;
            }

            selectedIndex--;
        }

        return false;
    }

    //위에 메서드를 활용하여 무기 스폰 시도
    private bool TrySpawnWeapon()
    {
        WeaponData selectedWeapon = SelectWeightedWeapon();

        if (selectedWeapon == null)
        {
            return false;
        }

        if (!TrySelectSpawnPoint(out Transform selectedPoint))
        {
            return false;
        }

        //비동기 생성이 끝나기 전부터 위치 예약
        occupiedSpawnPoints.Add(selectedPoint);

        int requestedVersion = spawnVersion;

        weaponPickupPrefab.InstantiateAsync(selectedPoint, false).Completed += handle =>
        {
            //Addressables 생성 실패
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                if(this != null && requestedVersion == spawnVersion)
                {
                    occupiedSpawnPoints.Remove(selectedPoint);
                }
          
                Addressables.Release(handle);
                return;
            }

            //생성이 끝나기 전에 WeaponSpawner가 파괴된 경우
            if (this == null)
            {
                Addressables.ReleaseInstance(handle.Result);
                return;
            }

            //이전 매치의 요청이거나 스포너가 중지된 경우
            if (requestedVersion != spawnVersion || !isRunning)
            {
                Addressables.ReleaseInstance(handle.Result);
                return;
            }


            GameObject pickupObject = handle.Result;

            pickupObject.transform.localPosition = Vector3.zero;
            pickupObject.transform.localRotation = Quaternion.identity;

            WeaponPickup pickup = pickupObject.GetComponent<WeaponPickup>();

            // 프리팹 구성 오류
            if (pickup == null)
            {
                occupiedSpawnPoints.Remove(selectedPoint);
                Addressables.ReleaseInstance(pickupObject);
                return;
            }

            // 선택된 무기 데이터로 픽업 초기화
            if (!pickup.Initialize(selectedWeapon))
            {
                occupiedSpawnPoints.Remove(selectedPoint);
                Addressables.ReleaseInstance(pickupObject);
                return;
            }

            pickup.PickedUp += HandlePickedUp;
            activePickups.Add(pickup);
        };

        return true;
    }

    //스폰을 멈추고 정리
    public void StopAndClear()
    {
        isRunning = false;

        currentMatch = null;
        spawnTimer = 0f;

        //진행 중인 비동기 요청을 이전 작업으로 만듦
        spawnVersion++;

        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            WeaponPickup pickup = activePickups[i];

            if (pickup == null)
            {
                continue;
            }

            pickup.PickedUp -= HandlePickedUp;
            Addressables.ReleaseInstance(pickup.gameObject);
        }

        activePickups.Clear();
        occupiedSpawnPoints.Clear();
    }


}
