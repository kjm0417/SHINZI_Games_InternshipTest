using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchHUDUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthBarUI playerHealthBar;
    [SerializeField] private HealthBarUI aiHealthBar;

    [Header("Match Info")]
    [SerializeField] private TMP_Text matchNameText;
    [SerializeField] private TMP_Text matchTimeText;
    [SerializeField] private TMP_Text aiNameText;
    [SerializeField] private TMP_Text playerWinsText;

    private MatchManager matchManager;
    private int lastDisplayedSecond = -1;

    public void Bind(MatchManager manager, CharacterHealthSystem playerHealth, CharacterHealthSystem aiHealth)
    {
        matchManager = manager;

        playerHealthBar.Bind(playerHealth);
        aiHealthBar.Bind(aiHealth);

        matchNameText.text = matchManager.CurrentMatch.MatchName;
        aiNameText.text = matchManager.CurrentMatch.AI_Id.AIName;
        playerWinsText.text = $"{matchManager.PlayerWins}½Â";

        lastDisplayedSecond = -1;
        UpdateTimerText();
    }

    public void Unbind()
    {
        playerHealthBar.Unbind();
        aiHealthBar.Unbind();

        matchManager = null;
        lastDisplayedSecond = -1;
    }

    private void Update()
    {
        if (matchManager == null || !matchManager.IsMatchRunning)
        {
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int totalSeconds = Mathf.CeilToInt(matchManager.RemainingTime);

        if (totalSeconds == lastDisplayedSecond)
        {
            return;
        }

        lastDisplayedSecond = totalSeconds;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        matchTimeText.text = $"{minutes:00}:{seconds:00}";
    }
}
