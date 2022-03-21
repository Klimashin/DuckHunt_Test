using System.Collections;
using UnityEngine;

public class RayWeapon : ShootingRangeWeapon
{
    [SerializeField] private LayerMask _interactionsLayer;
    [SerializeField] private float AnimationDelay = 0.2f;

    public override bool CanShot => _shotCoroutine == null;

    private Coroutine _shotCoroutine;
    private YieldInstruction _animationDelay;

    private void Awake()
    {
        _animationDelay = new WaitForSeconds(AnimationDelay);
    }

    public override void Shot(Ray shotRay)
    {
        _shotCoroutine = StartCoroutine(ShotCoroutine(shotRay));
    }

    private IEnumerator ShotCoroutine(Ray shotRay)
    {
        yield return _animationDelay; // animation, delays and stuff

        _shotCoroutine = null;
        
        var raycastHit = Physics2D.GetRayIntersection(shotRay,_interactionsLayer);
        if (!raycastHit || !raycastHit.collider.TryGetComponent<ShootingRangeTarget>(out var target))
        {
            yield break;
        }

        target.Hit();
    }
}
