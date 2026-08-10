using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewMatchDrop", menuName ="MatchDropData/MatchDrop")]
public class MatchDropData : ScriptableObject
{
    [SerializeField] private string dropListId;
    [SerializeField] private List<DropEntry> entries;
    [SerializeField] private int dropRate; //가중치 중 몇개로 계산하기 위해 int

    public string DropListId => dropListId;
    public List<DropEntry> En => entries;
    public int DropRate => dropRate;
}

[System.Serializable]
public class DropEntry
{
    [SerializeField] private WeaponData weapon;
    [SerializeField] private int dropRate;
}
