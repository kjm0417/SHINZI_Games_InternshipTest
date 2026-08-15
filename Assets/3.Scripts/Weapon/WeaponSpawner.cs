using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

    //Spanwer 시간
    private float spawnTimer;
    private bool isRunning;

    private MatchData currentMatch;

    public bool Initialize(MatchData matchData)
    {
        if (matchData == null) return false;

        if (matchData.DropId == null) return false;

        currentMatch = matchData;

        //첫 무기는 매치 시작 직후 생성
        spawnTimer = 0f;
        isRunning = true;

        return true;
    }

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
}
