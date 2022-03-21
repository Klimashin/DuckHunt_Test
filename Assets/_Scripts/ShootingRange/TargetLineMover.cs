public class TargetLineMover : TargetMover
{
    protected override void Move(float deltaTime)
    {
        transform.Translate(moveDirection * Speed * deltaTime);
    }
}
