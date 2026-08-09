using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayer", menuName ="PlayerData/Player")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private string playerId;
    [SerializeField] private int maxHp;
    [SerializeField] private int speed;
    [SerializeField] private int dashSpeed;
    [SerializeField] private float dashCooldown;


    [SerializeField] private string prefabAddressable;

    public string PlayerId => playerId;
    public int MaxHp => maxHp;
    public int Speed => speed;
    public int DashSpeed => dashSpeed;
    public float DashCooldown => dashCooldown;
}