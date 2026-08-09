using UnityEngine;

[CreateAssetMenu(fileName ="NewAI",menuName ="AIData/AI")]
public class AIData : ScriptableObject
{
    [SerializeField] private string aiId;
    [SerializeField] private string aiName;
    [SerializeField] private int maxHp;
    [SerializeField] private int speed;
    [SerializeField] private int dashSpeed;
    [SerializeField] private float dashCooldown;
    [SerializeField] private AIBehaviorData behaviorId;


    [SerializeField] private string prefabAdressable;

    public string AI_Id => aiId;
    public string AIName => aiName;
    public int MaxHp => maxHp;
    public int Speed => speed;
    public int DashSpeed => dashSpeed;
    public float DashCooldown => dashCooldown;
    public AIBehaviorData BehaviorId => behaviorId;

    public string PrefabAdressable => prefabAdressable;

}
