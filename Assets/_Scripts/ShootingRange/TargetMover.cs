using UnityEngine;

public abstract class TargetMover : MonoBehaviour
{
    
    [SerializeField] private float BaseSpeed = 1f;

    protected float Speed => BaseSpeed * moveSpeedCoefficient;
    
    protected Vector3 moveDirection;
    protected float moveSpeedCoefficient;

    public void Init(Vector3 direction, float speedCoefficient)
    {
        moveDirection = direction;
        moveSpeedCoefficient = speedCoefficient;
    }

    private void Update()
    {
        Move(Time.deltaTime);
    }

    protected abstract void Move(float deltaTime);
}
