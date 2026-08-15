using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectile", menuName = "ProjectileData/Projectile")]
public class ProjectileData : ScriptableObject
{
    [SerializeField] private string projectileId;
    [SerializeField] private string prefabAddressable;
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;

    public string ProjectileId => projectileId;
    public string PrefabAddressable => prefabAddressable;
    public float Speed => speed;
    public float LifeTime => lifeTime;
}