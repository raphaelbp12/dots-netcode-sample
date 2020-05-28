using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public struct ServerCubeGhostSerializer : IGhostSerializer<ServerCubeSnapshotData>
{
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypePhysicsDamping;
    private ComponentType componentTypePhysicsMass;
    private ComponentType componentTypePhysicsVelocity;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<ServerCubeSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypePhysicsDamping = ComponentType.ReadWrite<PhysicsDamping>();
        componentTypePhysicsMass = ComponentType.ReadWrite<PhysicsMass>();
        componentTypePhysicsVelocity = ComponentType.ReadWrite<PhysicsVelocity>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref ServerCubeSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
