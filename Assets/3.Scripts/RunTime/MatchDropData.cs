using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewMatchDrop", menuName ="MatchDropData/MatchDrop")]
public class MatchDropData : ScriptableObject
{
    [SerializeField] private string dropListId;
    [SerializeField] private List<DropEntry> entries;
  
    public string DropListId => dropListId;
    public IReadOnlyList<DropEntry> Entries => entries;
}

[System.Serializable]
public class DropEntry
{
    [SerializeField] private WeaponData weaponId;
    [SerializeField] private int dropRate;

    public WeaponData WeaponId => weaponId;
    public int DropRate => dropRate;
}
