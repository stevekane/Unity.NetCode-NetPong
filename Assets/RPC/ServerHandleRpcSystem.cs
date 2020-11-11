using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerHandleRpcSystem : SystemBase {
  EntityQuery ExistingPlayers;
  EntityQuery PaddleSpawns;

  protected override void OnCreate() {
    ExistingPlayers = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<Paddle>()
    });
    PaddleSpawns = EntityManager.CreateEntityQuery(new ComponentType[] { 
      ComponentType.ReadOnly<PaddleSpawn>(),
      ComponentType.ReadOnly<LocalToWorld>()
    });
    RequireSingletonForUpdate<NetworkIdComponent>();
  }

  protected override void OnUpdate() {
    var barrier = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    var ecb = barrier.CreateCommandBuffer();
    var subSceneReferences = GetSingleton<SubSceneReferences>();
    var prefabs = GetSingleton<EntityPrefabs>();
    var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>(isReadOnly: true);
    var networkStreamInGameFromEntity = GetComponentDataFromEntity<NetworkStreamInGame>(isReadOnly: true);
    var existingPlayerCount = ExistingPlayers.CalculateEntityCount();
    var paddleSpawns = PaddleSpawns.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
    var joinGameAck = new RpcJoinGameAck { 
      GhostsSubSceneGUID = subSceneReferences.GhostPrefabs,
      BoardSubSceneGUID = subSceneReferences.Board
    };

    Entities
    .WithAll<RpcJoinGame>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      if (networkStreamInGameFromEntity.HasComponent(request.SourceConnection)) {
        return;
      }

      var networkId = networkIdFromEntity[request.SourceConnection].Value;
      var playerEntity = ecb.Instantiate(prefabs.Paddle);
      var ackEntity = ecb.CreateEntity();
      var spawnIndex = (existingPlayerCount + 1) % 2;
      var spawnLocation = paddleSpawns[spawnIndex];

      ecb.SetComponent(playerEntity, new Translation { Value = spawnLocation.Position });
      ecb.SetComponent(playerEntity, new GhostOwnerComponent { NetworkId = networkId });
      ecb.SetComponent(request.SourceConnection, new CommandTargetComponent { targetEntity = playerEntity });
      ecb.AddComponent<NetworkStreamInGame>(request.SourceConnection);
      ecb.AddComponent<RpcJoinGameAck>(ackEntity);
      ecb.SetComponent(ackEntity, joinGameAck);
      ecb.AddComponent<SendRpcCommandRequestComponent>(ackEntity);
      ecb.SetComponent(ackEntity, new SendRpcCommandRequestComponent { TargetConnection = request.SourceConnection });
      ecb.DestroyEntity(requestEntity);
      UnityEngine.Debug.Log($"Allowing player {networkId} to join game.");
    })
    .WithDisposeOnCompletion(paddleSpawns)
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