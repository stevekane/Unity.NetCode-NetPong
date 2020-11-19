using Unity.Entities;
using Unity.NetCode;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerHandleRpcSystem : SystemBase {
  EntityQuery ExistingPlayers;
  EntityQuery PaddleSpawns;

  public static Entity CreateRpc(EntityCommandBuffer ecb, Entity targetConnection) {
    var entity = ecb.CreateEntity();
    var sendRpc = new SendRpcCommandRequestComponent { TargetConnection = targetConnection };

    ecb.AddComponent(entity, sendRpc);
    return entity;
  }

  public static Entity CreatePlayerEntity(EntityCommandBuffer ecb, Entity prefab, int existingPlayerCount, int networkId) {
    var entity = ecb.Instantiate(prefab);

    ecb.SetComponent(entity, new TeamOwner { TeamIndex = (existingPlayerCount % 2) });
    ecb.SetComponent(entity, new Paddle { Radians = (existingPlayerCount % 2) * PI });
    ecb.SetComponent(entity, new GhostOwnerComponent { NetworkId = networkId });
    return entity;
  }

  protected override void OnCreate() {
    ExistingPlayers = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<Paddle>()
    });
    RequireSingletonForUpdate<NetworkIdComponent>();
    RequireSingletonForUpdate<EntityPrefabs>();
  }

  protected override void OnUpdate() {
    var barrier = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    var ecb = barrier.CreateCommandBuffer();
    var subSceneReferences = SubSceneReferencesSingleton.Instance;
    var staticGeometryGUID = subSceneReferences.StaticGeometry.SceneGUID;
    var prefabs = GetSingleton<EntityPrefabs>();
    var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>(isReadOnly: true);
    var networkStreamInGameFromEntity = GetComponentDataFromEntity<NetworkStreamInGame>(isReadOnly: true);
    var existingPlayerCount = ExistingPlayers.CalculateEntityCount();

    Entities
    .WithAll<RpcRequestLevel>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      var networkId = networkIdFromEntity[request.SourceConnection].Value;
      var rpcEntity = CreateRpc(ecb, request.SourceConnection);

      ecb.AddComponent(rpcEntity, new RpcsRequestLevelAck { LevelGUID = staticGeometryGUID });
      ecb.DestroyEntity(requestEntity);
      UnityEngine.Debug.Log($"Player {networkId} requested level to load. Sending {staticGeometryGUID}");
    })
    .WithReadOnly(networkIdFromEntity)
    .WithBurst()
    .Schedule();

    Entities
    .WithAll<RpcJoinGame>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      if (networkStreamInGameFromEntity.HasComponent(request.SourceConnection)) {
        return;
      }

      var networkId = networkIdFromEntity[request.SourceConnection].Value;
      var playerEntity = CreatePlayerEntity(ecb, prefabs.Paddle, existingPlayerCount, networkId);
      var rpcEntity = CreateRpc(ecb, request.SourceConnection);

      ecb.AddComponent(rpcEntity, default(RpcJoinGameAck));
      ecb.SetComponent(request.SourceConnection, new CommandTargetComponent { targetEntity = playerEntity });
      ecb.AddComponent<NetworkStreamInGame>(request.SourceConnection);
      ecb.DestroyEntity(requestEntity);
      UnityEngine.Debug.Log($"Allowing player {networkId} to join game.");
    })
    .WithReadOnly(networkIdFromEntity)
    .WithReadOnly(networkStreamInGameFromEntity)
    .WithBurst()
    .Schedule();
    barrier.AddJobHandleForProducer(Dependency);
  }
}