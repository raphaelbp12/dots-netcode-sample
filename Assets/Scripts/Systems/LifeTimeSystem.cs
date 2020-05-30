using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class LifeTimeSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationECB;
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var ecb = endSimulationECB.CreateCommandBuffer().ToConcurrent();

        var deltaTime = Time.DeltaTime;

        var updateLifeTimeJob = Entities.ForEach((ref LifeTimeComponent lifeTimeComponent) => {
            lifeTimeComponent.Value -= deltaTime;
        }).ScheduleParallel(this.Dependency);

        var destroyJob = Entities.ForEach((Entity entity, int entityInQueryIndex, ref LifeTimeComponent lifeTimeComponent) => {
            if (lifeTimeComponent.Value < 0)
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel(updateLifeTimeJob);

        destroyJob.Complete();

        this.Dependency = destroyJob;
    }
}