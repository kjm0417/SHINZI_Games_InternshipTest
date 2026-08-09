using UnityEngine;

[CreateAssetMenu(fileName ="NewMatchDrop", menuName ="MatchDropData/MatchDrop")]
public class MatchDropData : ScriptableObject
{
    [SerializeField] private string dropListId;
    [SerializeField] private WeaponData weaponId;
    [SerializeField] private int dropRate; //가중치 중 몇개로 계산하기 위해 int

    public string DropListId => dropListId;
    public WeaponData WeaponId => WeaponId;
    public int DropRate => dropRate;
}
