using UnityEngine;

public abstract class ShootingRangeWeapon : MonoBehaviour
{
    public virtual bool CanShot => true; 
    
    public abstract void Shot(Ray shotRay);
}

public enum WeaponType
{
    RayWeapon = 0
}
