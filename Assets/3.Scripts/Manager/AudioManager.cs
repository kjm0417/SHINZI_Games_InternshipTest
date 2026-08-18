using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("피격음")]
    [SerializeField] private AudioClip hitSound;

    [Header("무기 공격음 (무기 id <-> 사운드)")]
    [SerializeField] private WeaponSound[] weaponSounds;

    private Dictionary<string, AudioClip> weaponSoundMap;

    private AsyncOperationHandle<AudioClip> bgmHandle;
    private bool hasBgmLoaded;

    [System.Serializable]
    public class WeaponSound
    {
        public string weaponId; //WeaponData id와 일치
        public AudioClip attackSound;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject); return; 
        }
        Instance = this;

        // 무기 사운드 맵 구축
        weaponSoundMap = new Dictionary<string, AudioClip>();
        foreach (var ws in weaponSounds)
        {
            if (ws != null && !string.IsNullOrEmpty(ws.weaponId))
            {
                weaponSoundMap[ws.weaponId] = ws.attackSound;
            }
        }
    }

    //배경음 Addressable
    public void PlayBGM(string bgmAddress)
    {
        if (string.IsNullOrEmpty(bgmAddress)) return;
        StopBGM();

        Addressables.LoadAssetAsync<AudioClip>(bgmAddress).Completed += handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"BGM 로드 실패: {bgmAddress}");
                Addressables.Release(handle);
                return;
            }

            if (this == null)
            {
                Addressables.Release(handle); return; 
            }

            bgmHandle = handle;
            hasBgmLoaded = true;
            bgmSource.clip = handle.Result;
            bgmSource.loop = true;
            bgmSource.Play();
        };
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
        if (hasBgmLoaded)
        {
            Addressables.Release(bgmHandle);
            hasBgmLoaded = false;
        }
    }

    // 무기 공격음 id로 재생
    public void PlayWeaponSound(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return;

        if (weaponSoundMap.TryGetValue(weaponId, out var clip) && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 피격음
    public void PlayHitSound()
    {
        if (hitSound != null)
        {
            sfxSource.PlayOneShot(hitSound);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) { StopBGM(); Instance = null; }
    }
}