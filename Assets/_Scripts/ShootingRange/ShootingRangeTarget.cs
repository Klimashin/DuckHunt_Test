using UnityEngine;

[RequireComponent(typeof(TargetMover))]
public abstract class ShootingRangeTarget : MonoBehaviour
{
    public event ShootingTargetNotification OnHit;
    public event ShootingTargetNotification OnLost;
    public event ShootingTargetNotification OnCleanup;

    public void Hit()
    {
        OnHit?.Invoke(this);
        CleanUp();
    }

    public void Lost()
    {
        OnLost?.Invoke(this);
        CleanUp();
    }
    
    public void CleanUp()
    {
        OnCleanup?.Invoke(this);
        Destroy(gameObject);
    }
}

public enum TargetType
{
    BasicTarget = 0
}

public delegate void ShootingTargetNotification(ShootingRangeTarget target);