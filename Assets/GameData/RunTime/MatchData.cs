using UnityEngine;

[CreateAssetMenu(fileName ="NewMatchData", menuName ="MatchData/Match")]
public class MatchData : ScriptableObject
{
    [SerializeField] private string matchId;
    [SerializeField] private string matchName;
    [SerializeField] private float itemDropCoolTime;
    [SerializeField] private float matchTime;
    [SerializeField] private int minWins;
    [SerializeField] private AIData aiId;
    [SerializeField] private MatchDropData dropListId;

    [SerializeField] private string prefabAddressable;

    public string MatchId => matchId;
    public string MatchName => matchName;
    public float ItemDropCoolTime => itemDropCoolTime;
    public float MatchTime => matchTime;
    public int MinWins => minWins;
    public AIData AI_Id => aiId;
    public MatchDropData DropId => dropListId;
    public string PrefabAddressable => prefabAddressable;
}
