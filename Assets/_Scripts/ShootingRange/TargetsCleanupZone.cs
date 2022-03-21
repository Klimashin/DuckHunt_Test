using UnityEngine;

public class TargetsCleanupZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<ShootingRangeTarget>(out var target))
        {
            target.Lost();
        }
    }
}
