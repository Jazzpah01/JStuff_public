using UnityEngine;

[CreateAssetMenu(menuName = "Assets/Collider/Flags")]
public class ColliderFlags : ScriptableObject
{
    public bool Solid = false, Flower = false, Body = false, Unit = false, Player = false, Enemy = false, Projectile = false, Platform = false, Flammable = false;
}