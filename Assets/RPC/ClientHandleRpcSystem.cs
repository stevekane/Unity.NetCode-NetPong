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

    Entities
    .ForEach((Entity requestEntity, in RpcsLoadSubScene loadSubScene, in ReceiveRpcCommandRequestComponent request) => {
      var loadParameters = new SceneSystem.LoadParameters {
        Flags = SceneLoadFlags.LoadAdditive
      };

      UnityEngine.Debug.Log($"Server sent request to load SubScene {loadSubScene.SceneGUID}.");
      sceneSystem.LoadSceneAsync(loadSubScene.SceneGUID, loadParameters);
      EntityManager.DestroyEntity(requestEntity);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();

    // Using Run on the mainthread because there does not seem to be an ecb-friendly API for loading subscenes..
    Entities
    .ForEach((Entity requestEntity, in RpcJoinGameAck ack, in ReceiveRpcCommandRequestComponent request) => {
      UnityEngine.Debug.Log($"Player {networkId} successfully joined the server.");
      EntityManager.AddComponent<NetworkStreamInGame>(connectionEntity);
      EntityManager.DestroyEntity(requestEntity);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();

    Entities
    .WithAll<RpcLeaveGameAck>()
    .ForEach((Entity requestEntity, in ReceiveRpcCommandRequestComponent request) => {
      UnityEngine.Debug.Log($"LeaveGameAck from {request.SourceConnection.Index} recieved but not implemented.");
    })
    .WithBurst()
    .Schedule();
  }
}