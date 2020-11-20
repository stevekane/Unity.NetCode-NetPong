using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;
using Unity.Networking.Transport;

public struct ClientConnection : IComponentData {
  public enum State { 
    Disconnected, 
    Connecting,
    LevelRequestSent,
    LoadingLevel,
    JoinGameRequestSent,
    InGame
  }

  public State CurrentState;
  public ushort Port;
  public Hash128 LevelGUID;
}

public struct ClientConnectionEvent : IComponentData {
  public enum Name { 
    Connect, 
    Disconnect, 
    LoadLevel,
    JoinGame,
    JoinGameAcknowledged 
  }

  public Name EventName;
  public ushort Port;
  public Hash128 LevelGUID;
}

[UpdateInWorld(UpdateInWorld.TargetWorld.Client)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ClientHandleRpcSystem))]
public class ClientConnectionSystem : SystemBase {
  EntityQuery ClientConnectionEventsQuery;

  public static NetworkEndPoint NetworkEndPointForEnvironment(ushort port) {
    #if UNITY_EDITOR
    return NetworkEndPoint.Parse(ClientServerBootstrap.RequestedAutoConnect, port);
    #else
    var endPoint = NetworkEndPoint.LoopbackIpv4;

    endPoint.Port = port;
    return endPoint;
    #endif
  }

  public static Entity CreateConnectionEvent(EntityManager entityManager, ClientConnectionEvent connectionEvent) {
    var e = entityManager.CreateEntity(typeof(ClientConnectionEvent));

    entityManager.SetComponentData(e, connectionEvent);
    return e;
  }

  protected override void OnCreate() {
    ClientConnectionEventsQuery = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<ClientConnectionEvent>()
    });
    RequireSingletonForUpdate<ClientConnection>();
  }
  
  protected override void OnUpdate() {
    var connectionEntity = GetSingletonEntity<ClientConnection>();
    var connection = GetSingleton<ClientConnection>();
    var networkStream = World.GetExistingSystem<NetworkStreamReceiveSystem>();
    var sceneSystem = World.GetExistingSystem<SceneSystem>();
    var connectionEvents = ClientConnectionEventsQuery.ToComponentDataArray<ClientConnectionEvent>(Allocator.Temp);

    // TODO: The looping isn't really correct here... need to think about what should really happen
    // it doesn't really make sense to process multiple events in the same frame? maybe it does? confuse...
    // maybe treat the whole thing as a loop that runs until the queue is exhausted? not sure bout that...

    switch (connection.CurrentState) {
      case ClientConnection.State.Disconnected: {
        for (int i = 0; i < connectionEvents.Length; i++) {
          if (connectionEvents[i].EventName == ClientConnectionEvent.Name.Connect) {
            var port = connectionEvents[i].Port;
            var networkStreamConnectionEntity = networkStream.Connect(NetworkEndPointForEnvironment(port));

            UnityEngine.Debug.Log($"Recieved Connect event and now connecting.");
            SubSceneRequestSystem.CreateSubSceneUnloadRequest(EntityManager, connection.LevelGUID);
            connection.Port = port;
            connection.CurrentState = ClientConnection.State.Connecting;
            break;
          }
        }
      }
      break;

      case ClientConnection.State.Connecting: {
        if (HasSingleton<NetworkIdComponent>()) {
          var networkStreamConnectionEntity = GetSingletonEntity<NetworkStreamConnection>();
          var rpcEntity = ClientHandleRpcSystem.CreateRpc(EntityManager, networkStreamConnectionEntity);
    
          UnityEngine.Debug.Log($"Accquired NetworkID. Sending LevelRequest.");
          EntityManager.AddComponentData(rpcEntity, default(RpcRequestLevel));
          connection.CurrentState = ClientConnection.State.JoinGameRequestSent;
          connection.CurrentState = ClientConnection.State.LevelRequestSent;
        }
      }
      break;

      case ClientConnection.State.LevelRequestSent: {
        for (int i = 0; i < connectionEvents.Length; i++) {
          if (connectionEvents[i].EventName == ClientConnectionEvent.Name.LoadLevel) {
            var levelGUID = connectionEvents[i].LevelGUID;

            UnityEngine.Debug.Log($"Received LoadLevelRequest. Unloading {connection.LevelGUID} and loading {levelGUID}");
            SubSceneRequestSystem.CreateSubSceneUnloadRequest(EntityManager, connection.LevelGUID);
            SubSceneRequestSystem.CreateSubSceneLoadRequest(EntityManager, levelGUID);
            connection.LevelGUID = levelGUID;
            connection.CurrentState = ClientConnection.State.LoadingLevel;
            break;
          }
        }
      }
      break;

      case ClientConnection.State.LoadingLevel: {
        if (sceneSystem.IsSceneLoaded(sceneSystem.GetSceneEntity(connection.LevelGUID))) {
          var networkStreamConnectionEntity = GetSingletonEntity<NetworkStreamConnection>();
          var rpcEntity = ClientHandleRpcSystem.CreateRpc(EntityManager, networkStreamConnectionEntity);

          UnityEngine.Debug.Log($"Level {connection.LevelGUID} loaded. Sending JoinGame request.");
          EntityManager.AddComponentData(rpcEntity, default(RpcJoinGame));
          connection.CurrentState = ClientConnection.State.JoinGameRequestSent;
        }
      }
      break;

      case ClientConnection.State.JoinGameRequestSent: {
        var networkStreamConnectionEntity = GetSingletonEntity<NetworkStreamConnection>();

        for (int i = 0; i < connectionEvents.Length; i++) {
          if (connectionEvents[i].EventName == ClientConnectionEvent.Name.JoinGameAcknowledged) {
            UnityEngine.Debug.Log($"Received JoinGameAck. Going InGame.");
            connection.CurrentState = ClientConnection.State.InGame;
            EntityManager.AddComponent<NetworkStreamInGame>(networkStreamConnectionEntity);
            break;
          }
        }
      }
      break;

      case ClientConnection.State.InGame: {
        // UnityEngine.Debug.Log("You are ingame man.");
      }
      break;
    }
    EntityManager.SetComponentData(connectionEntity, connection);
    EntityManager.DestroyEntity(ClientConnectionEventsQuery);
  }
}