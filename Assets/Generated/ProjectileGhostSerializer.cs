using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public struct ProjectileGhostSerializer : IGhostSerializer<ProjectileSnapshotData>
{
    private ComponentType componentTypeLifeTimeComponent;
    private ComponentType componentTypeSpeedComponent;
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypeCompositeScale;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<LifeTimeComponent> ghostLifeTimeComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<SpeedComponent> ghostSpeedComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<ProjectileSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeLifeTimeComponent = ComponentType.ReadWrite<LifeTimeComponent>();
        componentTypeSpeedComponent = ComponentType.ReadWrite<SpeedComponent>();
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypeCompositeScale = ComponentType.ReadWrite<CompositeScale>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostLifeTimeComponentType = system.GetArchetypeChunkComponentType<LifeTimeComponent>(true);
        ghostSpeedComponentType = system.GetArchetypeChunkComponentType<SpeedComponent>(true);
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref ProjectileSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataLifeTimeComponent = chunk.GetNativeArray(ghostLifeTimeComponentType);
        var chunkDataSpeedComponent = chunk.GetNativeArray(ghostSpeedComponentType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetLifeTimeComponentValue(chunkDataLifeTimeComponent[ent].Value, serializerState);
        snapshot.SetSpeedComponentValue(chunkDataSpeedComponent[ent].Value, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
