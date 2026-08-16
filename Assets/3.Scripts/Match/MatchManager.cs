using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum MatchResult
{
    PlayerVictory,
    PlayerDefeat
}


public class MatchManager : MonoBehaviour
{
    [Header("매치 데이터들")]
    [SerializeField] private MatchData[] matchDataList;

    [Header("매치 시스템")]
    [SerializeField] private WeaponSpawner weaponSpawner;

    [Header("캐릭터 생성")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform aiSpawnPoint;

    //이벤트
    public event Action<MatchResult> MatchEnded; //매치 끝
    public event Action<CharacterHealthSystem, CharacterHealthSystem> MatchStarted; //시작할 때 ui에 정보 넘겨주기

    private MatchData currentMatch;

    //플레이어 관련
    private int playerWins;
    private GameObject playerInstance;
    private PlayerController playerController;
    private CharacterHealthSystem playerHealth;

    //ai관련
    private GameObject aiInstance;
    private AIController aiController;
    private CharacterHealthSystem aiHealth;

    private float remainingTime; //매치 시간
    private bool isMatchRunning; //매치 중인지

    private int matchVersion; //매치 버전

    //UI가 읽기 위해 프로퍼티 읽기 전용만들기
    public MatchData CurrentMatch => currentMatch;
    public int PlayerWins => playerWins;
    public float RemainingTime => remainingTime;
    public bool IsMatchRunning => isMatchRunning;


    private void Update()
    {
        if (!isMatchRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime > 0f)
        {
            return;
        }

        remainingTime = 0f;
        HandleTimeExpired();
    }

    private void SubscribeCharacterEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }

        if (aiHealth != null)
        {
            aiHealth.Died += HandleAIDied;
        }
    }

    private void UnsubscribeCharacterEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        if (aiHealth != null)
        {
            aiHealth.Died -= HandleAIDied;
        }
    }

    //플레이어 생성
    private void SpawnPlayer(int requestedVersion)
    {
        string playerAddress = playerData.PrefabAddressable;

        Addressables.InstantiateAsync(
            playerAddress, playerSpawnPoint.position, playerSpawnPoint.rotation).Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    return;
                }

                if (this == null || requestedVersion != matchVersion)
                {
                    Addressables.ReleaseInstance(handle.Result);
                    return;
                }

                PlayerController controller = handle.Result.GetComponent<PlayerController>();

                if (controller == null)
                {
                    Addressables.ReleaseInstance(handle.Result);
                    return;
                }

                if (!controller.Initialize(playerData))
                {
                    Addressables.ReleaseInstance(handle.Result);
                    return;
                }

                playerInstance = handle.Result;
                playerController = controller;
                playerHealth = controller.HealthSystem;

                SpawnAI(requestedVersion);
            };
            
    }

    //ai생성
    private void SpawnAI(int requestedVersion)
    {
        AIData selectedAIData = currentMatch.AI_Id;
        string aiAddress = selectedAIData.PrefabAdressable;

        Addressables.InstantiateAsync(
            aiAddress, aiSpawnPoint.position, aiSpawnPoint.rotation).Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    HandleMatchStartFailed();
                    return;
                }

                if (this == null || requestedVersion != matchVersion)
                {
                    Addressables.ReleaseInstance(handle.Result);
                    return;
                }

                AIController controller = handle.Result.GetComponent<AIController>();

                if (controller == null)
                {
                    Addressables.ReleaseInstance(handle.Result);
                    HandleMatchStartFailed();
                    return;
                }

                if (!controller.Initialize(
                    selectedAIData,
                    playerController.transform,
                    weaponSpawner))
                {
                    Addressables.ReleaseInstance(handle.Result);
                    HandleMatchStartFailed();
                    return;
                }

                aiInstance = handle.Result;
                aiController = controller;
                aiHealth = controller.HealthSystem;

                //게임 시작
                CompleteMatchStart(requestedVersion);
            };
       
    }

    public void StartNextMatch()
    {
        StartMatch();
    }

    public void StartGame()
    {
        StartMatch();
    }

    //매치 시작
    private void StartMatch()
    {
        isMatchRunning = false;

        currentMatch = SelectMatchData(playerWins);

        if (currentMatch == null)
        {
            Debug.LogError($"승수 {playerWins}에 해당하는 MatchData가 없습니다.");
            return;
        }

        matchVersion++;
        int requestedVersion = matchVersion;

        SpawnPlayer(requestedVersion);
    }

    //게임 끝나고 사용 ; 재시작용
  

    private void CompleteMatchStart(int requestedVersion)
    {
        if (requestedVersion != matchVersion) return;

        if (!weaponSpawner.Initialize(currentMatch))
        {
            Debug.LogError($"WeaponSpawner 초기화 실패: {currentMatch.MatchId}");
            HandleMatchStartFailed();
            return;
        }

        SubscribeCharacterEvents();

        remainingTime = currentMatch.MatchTime;
        isMatchRunning = true;

        MatchStarted?.Invoke(playerHealth, aiHealth);
    }

    //매치 승수로 선택
    private MatchData SelectMatchData(int playerWins)
    {
        MatchData selectedMatch = null;

        for (int i = 0; i < matchDataList.Length; i++)
        {
            MatchData candidate = matchDataList[i];

            if (candidate == null)
            {
                continue;
            }

            if (candidate.MinWins > playerWins)
            {
                continue;
            }

            if (selectedMatch == null || candidate.MinWins > selectedMatch.MinWins)
            {
                selectedMatch = candidate;
            }
        }

        return selectedMatch;
    }

    //매치 끝
    private void EndMatch(MatchResult result)
    {
        if (!isMatchRunning)
        {
            return;
        }

        isMatchRunning = false;

        weaponSpawner.StopAndClear();
        UnsubscribeCharacterEvents();

        ReleaseCharacters();

        if (result == MatchResult.PlayerVictory)
        {
            playerWins++;
        }

        MatchEnded?.Invoke(result);
    }

    //시간이 끝나고 처리 
    private void HandleTimeExpired()
    {
        if (aiHealth.IsDead)
        {
            EndMatch(MatchResult.PlayerVictory);
            return;
        }

        EndMatch(MatchResult.PlayerDefeat);
    }


    //ai, player Addressables 해제
    private void ReleaseCharacters()
    {
        if(aiInstance != null)
        {
            Addressables.ReleaseInstance(aiInstance);
            aiInstance = null;
        }

        if(playerInstance != null)
        {
            Addressables.ReleaseInstance(playerInstance);
            playerInstance = null;
        }

        aiController = null;
        playerController = null;

        aiHealth = null;
        playerHealth = null;
    }

    //생성 실패 처리
    private void HandleMatchStartFailed()
    {
        isMatchRunning = false;

        weaponSpawner.StopAndClear();
        ReleaseCharacters();
    }

    #region 사망처리
    private void HandlePlayerDied()
    {
        EndMatch(MatchResult.PlayerDefeat);
    }

    private void HandleAIDied()
    {
        EndMatch(MatchResult.PlayerVictory);
    }

    #endregion


    //Test용
    [ContextMenu("Start Next Match")]
    public void StartNextMatch1()
    {
        StartMatch();
        Debug.Log($"매치 시작: {currentMatch.MatchId}, 현재 승수: {playerWins}");
    }
    
}
