using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class RotationSpeedSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation, ref RotationSpeed rotationSpeed) =>
        {
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.back(), rotationSpeed.RadiansPerSecond * Time.DeltaTime));
        });
    }
}