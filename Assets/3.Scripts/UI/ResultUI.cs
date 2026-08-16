using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;

    public void Show(MatchResult result, int playerWins)
    {
        if (result == MatchResult.PlayerVictory)
        {
            resultText.text = $"½Â¸®!\nÇöÀç {playerWins}½Â";
        }
        else
        {
            resultText.text = $"ÆÐ¹è\nÇöÀç {playerWins}½Â";
        }
    }
}
