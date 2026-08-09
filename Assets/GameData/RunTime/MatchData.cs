using UnityEngine;

[CreateAssetMenu(fileName ="NewMatchData", menuName ="MatchData/Match")]
public class MatchData : ScriptableObject
{
    [SerializeField] private string matchId;
    [SerializeField] private string matchName;
    [SerializeField] private float itemDropCoolTime;
    [SerializeField] private float matchTime;
    [SerializeField] private MatchProgressionData progressionId;

    [SerializeField] private string prefabAddressable;

    public string MatchId => matchId;
    public string MatchName => matchName;
    public float ItemDropCoolTime => itemDropCoolTime;
    public float MatchTime => matchTime;
    public string PrefabAddressable => prefabAddressable;
}
