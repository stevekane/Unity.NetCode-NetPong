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

  public static World CreateClientApplicationWorld(World clientWorld, string name) {
    var applicationWorld = new World("Client Application World");
    var clientMenuSystemGroup = applicationWorld.CreateSystem<ClientMenuSystemGroup>();
    var clientMenuSystem = applicationWorld.CreateSystem<ClientMenuSystem>();
    var rootApplicationSystemTypes = new Type[1] { typeof(SimulationSystemGroup) };

    clientWorld.EntityManager.CreateEntity(typeof(ClientConnection));
    clientMenuSystem.ClientWorld = clientWorld;
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

    // Server initialization
    if (RequestedPlayType != PlayType.Client) {
      var serverWorld = CreateServerWorld(defaultWorld, "Server World");
      var networkStream = serverWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
      var endPoint = NetworkEndPoint.AnyIpv4;

      // SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.StaticGeometry.SceneGUID);
      // SubSceneRequestSystem.CreateSubSceneLoadRequest(serverWorld.EntityManager, subSceneReferences.GameState.SceneGUID);
      endPoint.Port = DEFAULT_PORT;
      networkStream.Listen(endPoint);
      UnityEngine.Debug.Log($"Server listening on port {endPoint.Port}.");
    }

    // Client initialization
    if (RequestedPlayType != PlayType.Server) {
      var clientWorld = CreateClientWorld(defaultWorld, "Client World");
      var applicationWorld = CreateClientApplicationWorld(clientWorld, "Client Application World");
      var gameObjectPrefabs = Resources.LoadAll<GameObject>("");
      var audioClips = Resources.LoadAll<AudioClip>("");
      var gameObjectCache = GameObjectCache.FromResources(gameObjectPrefabs);
      var audioClipCache = AudioClipCache.FromResources(audioClips);
      var cachesEntity = clientWorld.EntityManager.CreateEntity(typeof(GameObjectCache), typeof(AudioClipCache));

      clientWorld.EntityManager.AddComponentData(cachesEntity, gameObjectCache);
      clientWorld.EntityManager.AddComponentData(cachesEntity, audioClipCache);
    }

    return true;
  }
}