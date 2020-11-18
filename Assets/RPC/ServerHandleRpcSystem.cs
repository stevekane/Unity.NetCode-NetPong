using Unity.Entities;
using Unity.NetCode;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerHandleRpcSystem : SystemBase {
  EntityQuery ExistingPlayers;
  EntityQuery PaddleSpawns;

  public static Entity CreateLoadSubSceneRequest(EntityCommandBuffer ecb, Entity targetConnection, Hash128 sceneGUID) {
    var entity = ecb.CreateEntity();

    ecb.AddComponent<RpcsLoadSubScene>(entity);
    ecb.AddComponent<SendRpcCommandRequestComponent>(entity);
    ecb.SetComponent(entity, new RpcsLoadSubScene { SceneGUID = sceneGUID });
    ecb.SetComponent(entity, new SendRpcCommandRequestComponent { TargetConnection = targetConnection });
    return entity;
  }

  public static Entity CreateJoinGameAckRequest(EntityCommandBuffer ecb, Entity targetConnection) {
    var entity = ecb.CreateEntity();

    ecb.AddComponent<RpcJoinGameAck>(entity);
    ecb.AddComponent<SendRpcCommandRequestComponent>(entity);
    ecb.SetComponent(entity, default(RpcJoinGameAck));
    ecb.SetComponent(entity, new SendRpcCommandRequestComponent { TargetConnection = targetConnection });
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
    var sharedResourcesGUID = subSceneReferences.SharedResources.SceneGUID;
    var staticGeometryGUID = subSceneReferences.StaticGeometry.SceneGUID;
    var prefabs = GetSingleton<EntityPrefabs>();
    var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>(isReadOnly: true);
    var networkStreamInGameFromEntity = GetComponentDataFromEntity<NetworkStreamInGame>(isReadOnly: true);
    var existingPlayerCount = ExistingPlayers.CalculateEntityCount();

    Entities
    .WithAll<RpcJoinGame>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      if (networkStreamInGameFromEntity.HasComponent(request.SourceConnection)) {
        return;
      }

      var networkId = networkIdFromEntity[request.SourceConnection].Value;
      var playerEntity = CreatePlayerEntity(ecb, prefabs.Paddle, existingPlayerCount, networkId);

      CreateLoadSubSceneRequest(ecb, request.SourceConnection, sharedResourcesGUID);
      CreateLoadSubSceneRequest(ecb, request.SourceConnection, staticGeometryGUID);
      CreateLoadSubSceneRequest(ecb, request.SourceConnection, staticGeometryGUID);
      CreateJoinGameAckRequest(ecb, request.SourceConnection);
      ecb.SetComponent(request.SourceConnection, new CommandTargetComponent { targetEntity = playerEntity });
      ecb.AddComponent<NetworkStreamInGame>(request.SourceConnection);
      ecb.DestroyEntity(requestEntity);
      UnityEngine.Debug.Log($"Allowing player {networkId} to join game.");
    })
    .WithReadOnly(networkIdFromEntity)
    .WithReadOnly(networkStreamInGameFromEntity)
    .WithBurst()
    .Schedule();

    Entities
    .WithAll<RpcLeaveGame>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      UnityEngine.Debug.Log($"LeaveGame from {request.SourceConnection.Index} recieved but not implemented");
    })
    .WithBurst()
    .Schedule();
    barrier.AddJobHandleForProducer(Dependency);
  }
}