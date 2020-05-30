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
            "ProjectileGhostSerializer",
        };
        return arr;
    }

    public int Length => 3;
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
        var curProjectileGhostSpawnSystem = world.GetOrCreateSystem<ProjectileGhostSpawnSystem>();
        m_ProjectileSnapshotDataNewGhostIds = curProjectileGhostSpawnSystem.NewGhostIds;
        m_ProjectileSnapshotDataNewGhosts = curProjectileGhostSpawnSystem.NewGhosts;
        curProjectileGhostSpawnSystem.GhostType = 2;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_CubeSnapshotDataFromEntity = system.GetBufferFromEntity<CubeSnapshotData>();
        m_ServerCubeSnapshotDataFromEntity = system.GetBufferFromEntity<ServerCubeSnapshotData>();
        m_ProjectileSnapshotDataFromEntity = system.GetBufferFromEntity<ProjectileSnapshotData>();
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
            case 2:
                return GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeDeserialize(m_ProjectileSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
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
            case 2:
                m_ProjectileSnapshotDataNewGhostIds.Add(ghostId);
                m_ProjectileSnapshotDataNewGhosts.Add(GhostReceiveSystem<dotsnetcodesampleGhostDeserializerCollection>.InvokeSpawn<ProjectileSnapshotData>(snapshot, ref reader, compressionModel));
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
    private BufferFromEntity<ProjectileSnapshotData> m_ProjectileSnapshotDataFromEntity;
    private NativeList<int> m_ProjectileSnapshotDataNewGhostIds;
    private NativeList<ProjectileSnapshotData> m_ProjectileSnapshotDataNewGhosts;
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
