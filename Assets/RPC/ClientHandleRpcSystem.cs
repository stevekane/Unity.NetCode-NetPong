using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class ClientHandleRpcSystem : SystemBase {
  protected override void OnCreate() {
    RequireSingletonForUpdate<NetworkIdComponent>();
  }

  protected override void OnUpdate() {
    var networkId = GetSingleton<NetworkIdComponent>().Value;
    var connectionEntity = GetSingletonEntity<NetworkStreamConnection>();
    var sceneSystem = World.GetExistingSystem<SceneSystem>();

    // Using Run on the mainthread because there does not seem to be an ecb-friendly API for loading subscenes..
    Entities
    .ForEach((Entity requestEntity, in RpcJoinGameAck ack, in ReceiveRpcCommandRequestComponent request) => {
      var ghostsGUID = ack.GhostsSubSceneGUID;
      var boardGUID = ack.BoardSubSceneGUID;

      UnityEngine.Debug.Log($"Player {networkId} successfully joined the server.");
      UnityEngine.Debug.Log($"Server requested to load {ghostsGUID} and {boardGUID}.");
      
      sceneSystem.LoadSceneAsync(ghostsGUID);
      sceneSystem.LoadSceneAsync(boardGUID);
      EntityManager.DestroyEntity(requestEntity);
      EntityManager.AddComponent<NetworkStreamInGame>(connectionEntity);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();

    Entities
    .WithAll<RpcLeaveGameAck>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      UnityEngine.Debug.Log($"LeaveGameAck from {request.SourceConnection.Index} recieved but not implemented");
    })
    .WithBurst()
    .Schedule();
  }
}