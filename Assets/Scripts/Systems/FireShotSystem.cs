using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class FireShotSystem : ComponentSystem
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
        var ecb = endSimulationECB.CreateCommandBuffer();

        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = dotsnetcodesampleGhostSerializerCollection.FindGhostType<ProjectileSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;

        Entities.ForEach((DynamicBuffer<CubeInput> inputBuffer, ref Translation trans, ref Rotation rot) =>
        {
            CubeInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            if (input.fire == 1 || input.toggleFire == 1)
            {
                var projectile = ecb.Instantiate(prefab);

                ecb.SetComponent(projectile, trans);
                ecb.SetComponent(projectile, rot);
            }
        });
    }
}