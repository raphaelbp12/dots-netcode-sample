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
            "ServerCubeGhostSerializer",
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
        var curServerCubeGhostSpawnSystem = world.GetOrCreateSystem<ServerCubeGhostSpawnSystem>();
        m_ServerCubeSnapshotDataNewGhostIds = curServerCubeGhostSpawnSystem.NewGhostIds;
        m_ServerCubeSnapshotDataNewGhosts = curServerCubeGhostSpawnSystem.NewGhosts;
        curServerCubeGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_CubeSnapshotDataFromEntity = system.GetBufferFromEntity<CubeSnapshotData>();
        m_ServerCubeSnapshotDataFromEntity = system.GetBufferFromEntity<ServerCubeSnapshotData>();
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
                return GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeDeserialize(m_ServerCubeSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
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
                m_ServerCubeSnapshotDataNewGhostIds.Add(ghostId);
                m_ServerCubeSnapshotDataNewGhosts.Add(GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeSpawn<ServerCubeSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<CubeSnapshotData> m_CubeSnapshotDataFromEntity;
    private NativeList<int> m_CubeSnapshotDataNewGhostIds;
    private NativeList<CubeSnapshotData> m_CubeSnapshotDataNewGhosts;
    private BufferFromEntity<ServerCubeSnapshotData> m_ServerCubeSnapshotDataFromEntity;
    private NativeList<int> m_ServerCubeSnapshotDataNewGhostIds;
    private NativeList<ServerCubeSnapshotData> m_ServerCubeSnapshotDataNewGhosts;
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
