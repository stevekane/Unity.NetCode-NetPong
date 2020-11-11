using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;

public class NetPongClientServerBootstrap : ClientServerBootstrap {
  public static ushort DEFAULT_PORT = 7979;

  public override bool Initialize(string defaultWorldName) {
    World.DefaultGameObjectInjectionWorld = new World(defaultWorldName);
    GenerateSystemLists(DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default));
    DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld, ExplicitDefaultWorldSystems);
    #if !UNITY_DOTSRUNTIE
    ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
    #endif


    // Client-only initialization
    if (RequestedPlayType != PlayType.Server) {
      var defaultWorld = World.DefaultGameObjectInjectionWorld;
      var defaultSimulationSystemGroup = defaultWorld.GetExistingSystem<SimulationSystemGroup>();
      var clientMenuWorld = new World("Client Menu World");
      var clientMenuSystemGroup = clientMenuWorld.CreateSystem<ClientMenuSystemGroup>();
      var clientMenuSystem = clientMenuWorld.CreateSystem<ClientMenuSystem>();

      clientMenuSystemGroup.AddSystemToUpdateList(clientMenuSystem);
      defaultSimulationSystemGroup.AddSystemToUpdateList(clientMenuSystemGroup);
    }

    // Server-only initialization
    if (RequestedPlayType != PlayType.Client) {
      var defaultWorld = World.DefaultGameObjectInjectionWorld;
      var serverWorld = CreateServerWorld(defaultWorld, "ServerWorld");
      var networkStream = serverWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
      var endPoint = NetworkEndPoint.AnyIpv4;

      endPoint.Port = DEFAULT_PORT;
      networkStream.Listen(endPoint);
      UnityEngine.Debug.Log($"Server listening on port {endPoint.Port}");
    }
    return true;
  }
}