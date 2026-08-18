using UnityEngine;

[CreateAssetMenu(fileName = "NewAIBehaviorData", menuName = "AIBehaviorData/AIBehavior")]
public class AIBehaviorData : ScriptableObject
{
    [SerializeField] private string aiBehaviorId;
    [SerializeField] private float reactionTime;
    [SerializeField] private float dodgeChance;

    //읽기 전용 프로퍼티
    public string AIBehaviorId => aiBehaviorId;
    public float ReactionTime => reactionTime;
    public float DodgeChance => dodgeChance;
}
