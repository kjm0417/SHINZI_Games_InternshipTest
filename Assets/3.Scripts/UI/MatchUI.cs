using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchUI : MonoBehaviour
{
    [Header("매치")]
    [SerializeField] private MatchManager matchManager;

    [Header("Canvas")]
    [SerializeField] private GameObject gameStartCanvas;
    [SerializeField] private GameObject matchCanvas;
    [SerializeField] private GameObject resultCanvas;

    [Header("버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button nextMatchButton;
    [SerializeField] private Button ReStartButton;


    [Header("Match HUD")]
    [SerializeField] private MatchHUDUI matchHUD;

    [Header("Result UI")]
    [SerializeField] private ResultUI resultUI;


    private void Awake()
    {
        ShowGameStart();
        startButton.onClick.AddListener(OnStartButtonClicked);
        nextMatchButton.onClick.AddListener(OnNextMatchButtonClicked);
        ReStartButton.onClick.AddListener(OnReStartButtonClicked);

    }

    private void ShowGameStart()
    {
        gameStartCanvas.SetActive(true);
        matchCanvas.SetActive(false);
        resultCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        matchManager.MatchStarted += HandleMatchStarted;
        matchManager.MatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        matchManager.MatchStarted -= HandleMatchStarted;
        matchManager.MatchEnded -= HandleMatchEnded;
    }

    //시작 버튼 OnClick
    public void OnStartButtonClicked()
    {
        gameStartCanvas.SetActive(false);
        matchManager.StartGame();
    }

    private void HandleMatchStarted(PlayerController playerController,CharacterHealthSystem aiHealth)
    {
        matchHUD.Bind(matchManager, playerController, aiHealth);

        gameStartCanvas.SetActive(false);
        matchCanvas.SetActive(true);
        resultCanvas.SetActive(false);
    }

    private void HandleMatchEnded(MatchResult result)
    {
        matchHUD.Unbind();

        resultUI.Show(result, matchManager.PlayerWins);

        matchCanvas.SetActive(false);
        resultCanvas.SetActive(true);
    }

    // 다음 매치 버튼 OnClick
    public void OnNextMatchButtonClicked()
    {
        resultCanvas.SetActive(false);
        matchManager.StartNextMatch();
    }

    //다시 시작 버튼
    public void OnReStartButtonClicked()
    {
        resultCanvas.SetActive(false);
        matchManager.ReStart();
    }

}
