using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public class NetPongClientServerBootstrap : ClientServerBootstrap {
  public static ushort DEFAULT_PORT = 7979;

  public static World CreateDefaultWorld(string name) {
    var defaultWorld = new World(name);

    World.DefaultGameObjectInjectionWorld = defaultWorld;
    GenerateSystemLists(DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default));
    DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(defaultWorld, ExplicitDefaultWorldSystems);
    #if !UNITY_DOTSRUNTIME
    ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(defaultWorld);
    #endif
    return defaultWorld;
  }

  public static World CreateClientApplicationWorld(string name) {
    var applicationWorld = new World("Client Application World");
    var clientMenuSystemGroup = applicationWorld.CreateSystem<ClientMenuSystemGroup>();
    var clientMenuSystem = applicationWorld.CreateSystem<ClientMenuSystem>();
    var rootApplicationSystemTypes = new Type[1] { typeof(SimulationSystemGroup) };

    DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(applicationWorld, rootApplicationSystemTypes);
    applicationWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(clientMenuSystemGroup);
    clientMenuSystemGroup.AddSystemToUpdateList(clientMenuSystem);
    #if !UNITY_DOTSRUNTIME
    ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(applicationWorld);
    #endif
    return applicationWorld;
  }

  public override bool Initialize(string defaultWorldName) {
    var defaultWorld = CreateDefaultWorld("Default World");
    var subSceneReferences = GameObject.FindObjectOfType<SubSceneReferencesSingleton>().CreateInstance();

    switch (RequestedPlayType) {
    case PlayType.Client: {
      var applicationWorld = CreateClientApplicationWorld("Client Application World");
    }
    break;

    case PlayType.Server: {
      var serverWorld = CreateServerWorld(defaultWorld, "Server World");
      var networkStream = serverWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
      var endPoint = NetworkEndPoint.AnyIpv4;

      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.SharedResources);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.StaticGeometry);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.GameState);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.Ghosts);
      endPoint.Port = DEFAULT_PORT;
      networkStream.Listen(endPoint);
      UnityEngine.Debug.Log($"Server listening on port {endPoint.Port}.");
    }
    break;

    case PlayType.ClientAndServer: {
      var applicationWorld = CreateClientApplicationWorld("Client Application World");
      var serverWorld = CreateServerWorld(defaultWorld, "Server World");
      var networkStream = serverWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
      var endPoint = NetworkEndPoint.AnyIpv4;

      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.SharedResources);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.StaticGeometry);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.GameState);
      SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.Ghosts);
      endPoint.Port = DEFAULT_PORT;
      networkStream.Listen(endPoint);
      UnityEngine.Debug.Log($"Server listening on port {endPoint.Port}.");
    }
    break;
    }
    return true;
  }
}