using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class MoveSpeedSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((ref Translation trans, ref Rotation rot, ref SpeedComponent speedComponent) => {
            float3 front = new float3(0, 0, 1);
            trans.Value += math.mul(rot.Value, front) * speedComponent.Value;
        }).ScheduleParallel();
    }
}