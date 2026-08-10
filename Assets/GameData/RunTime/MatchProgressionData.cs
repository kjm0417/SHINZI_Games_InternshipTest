using UnityEngine;

[CreateAssetMenu(fileName = "NewMatchProgressionData", menuName ="MatchProgressionData/MatchProgression")]
public class MatchProgressionData : ScriptableObject
{
    [SerializeField] private string progressionId;
    [SerializeField] private int minWins;
    [SerializeField] private AIData aiId;
    [SerializeField] private MatchDropData dropListId;

    public string ProgressionId => progressionId;
    public int MinWins => minWins;
    public AIData AI_Id => aiId;
    public MatchDropData DropId => dropListId;
}
