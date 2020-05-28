using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct dotsnetcodesampleGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "CubeGhostSerializer",
            "CubeOnServerGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curCubeGhostSpawnSystem = world.GetOrCreateSystem<CubeGhostSpawnSystem>();
        m_CubeSnapshotDataNewGhostIds = curCubeGhostSpawnSystem.NewGhostIds;
        m_CubeSnapshotDataNewGhosts = curCubeGhostSpawnSystem.NewGhosts;
        curCubeGhostSpawnSystem.GhostType = 0;
        var curCubeOnServerGhostSpawnSystem = world.GetOrCreateSystem<CubeOnServerGhostSpawnSystem>();
        m_CubeOnServerSnapshotDataNewGhostIds = curCubeOnServerGhostSpawnSystem.NewGhostIds;
        m_CubeOnServerSnapshotDataNewGhosts = curCubeOnServerGhostSpawnSystem.NewGhosts;
        curCubeOnServerGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_CubeSnapshotDataFromEntity = system.GetBufferFromEntity<CubeSnapshotData>();
        m_CubeOnServerSnapshotDataFromEntity = system.GetBufferFromEntity<CubeOnServerSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeDeserialize(m_CubeSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 1:
                return GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeDeserialize(m_CubeOnServerSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_CubeSnapshotDataNewGhostIds.Add(ghostId);
                m_CubeSnapshotDataNewGhosts.Add(GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeSpawn<CubeSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 1:
                m_CubeOnServerSnapshotDataNewGhostIds.Add(ghostId);
                m_CubeOnServerSnapshotDataNewGhosts.Add(GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeSpawn<CubeOnServerSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<CubeSnapshotData> m_CubeSnapshotDataFromEntity;
    private NativeList<int> m_CubeSnapshotDataNewGhostIds;
    private NativeList<CubeSnapshotData> m_CubeSnapshotDataNewGhosts;
    private BufferFromEntity<CubeOnServerSnapshotData> m_CubeOnServerSnapshotDataFromEntity;
    private NativeList<int> m_CubeOnServerSnapshotDataNewGhostIds;
    private NativeList<CubeOnServerSnapshotData> m_CubeOnServerSnapshotDataNewGhosts;
}
public struct EnabledotsnetcodesampleGhostReceiveSystemComponent : IComponentData
{}
public class dotsnetcodesampleGhostReceiveSystem : GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnabledotsnetcodesampleGhostReceiveSystemComponent>();
    }
}
