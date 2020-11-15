#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;
using UnityEngine;

public enum MenuAction { 
  ExitTitleScreen,
  ExitGame,
  JoinGame
}

[DisableAutoCreation]
public class ClientMenuSystem : SystemBase {
  public static List<MenuAction> MenuActions = new List<MenuAction>(16);

  World ClientWorld;

  protected override void OnCreate() {
    EntityManager.CreateEntity(typeof(ClientMenuState));
    RequireSingletonForUpdate<ClientMenuState>();
  }

  protected override void OnUpdate() {
    var clientMenuStateEntity = GetSingletonEntity<ClientMenuState>();
    var state = GetComponent<ClientMenuState>(clientMenuStateEntity);

    if (state.IsLoading) {
      var loadingScene = SceneManager.GetSceneByName(state.LastLoadingMenuName.ToString());

      if (loadingScene.isLoaded) {
        foreach (var rootObject in loadingScene.GetRootGameObjects()) {
          if (rootObject.TryGetComponent(out Menu menu)) {
            menu.RegisterMenuActions(MenuActions);
          }
        }
        state.IsLoading = false;
        state.CurrentMenu = state.LastLoadingMenu;
      }
    }

    if (state.IsLoading) {
      return;
    }

    switch (state.CurrentMenu) {
    case ClientMenuState.Menu.Bootstrap:
      Load(ref state, ClientMenuState.Menu.TitleScreen, "Title Screen");
    break;

    case ClientMenuState.Menu.TitleScreen:
      ProcessTitleScreenActions(ref state, MenuActions);
    break;

    case ClientMenuState.Menu.MainMenu:
      ProcessMainMenuActions(ref state, MenuActions);
    break;

    case ClientMenuState.Menu.ConnectingToServer:
      UpdateConnectingToServer(ref state);
    break;

    case ClientMenuState.Menu.JoiningGame:
      UpdateJoiningGame(ref state);
    break;

    case ClientMenuState.Menu.InGame:
      UpdateInGame(ref state);
    break;
    }
    MenuActions.Clear();
    SetComponent(clientMenuStateEntity, state);
  }

  void Load(ref ClientMenuState state, ClientMenuState.Menu menu, FixedString128 name) {
    state.IsLoading = true;
    state.LastLoadingMenu = menu;
    state.LastLoadingMenuName = name;
    SceneManager.LoadScene(name.ToString(), LoadSceneMode.Additive);
  }

  void Unload(string name) {
    SceneManager.UnloadSceneAsync(name, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
  }

  void ProcessTitleScreenActions(ref ClientMenuState state, List<MenuAction> actions) {
    foreach (var action in actions) {
      switch (action) {
      case MenuAction.ExitTitleScreen:
        Unload("Title Screen");
        Load(ref state, ClientMenuState.Menu.MainMenu, "Main Menu");
      break;

      case MenuAction.ExitGame:
        Debug.Log("You quit!");
        QuitGame();
      break;
      }
    }
  }

  void ProcessMainMenuActions(ref ClientMenuState state, List<MenuAction> actions) {
    foreach (var action in actions) {
      switch (action) {
      case MenuAction.JoinGame:
        ConnectToServer(ref state, NetPongClientServerBootstrap.DEFAULT_PORT);
      break;

      case MenuAction.ExitGame:
        Debug.Log("You quit!");
        QuitGame();
      break;
      default:
      break;
      }
    }
  }

  void UpdateConnectingToServer(ref ClientMenuState state) {
    var networkStream = ClientWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
    var connectionEntity = ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection)).ToEntityArray(Allocator.Temp)[0];

    if (ClientWorld.EntityManager.HasComponent<NetworkIdComponent>(connectionEntity)) {
      var joinGameRequest = ClientWorld.EntityManager.CreateEntity();
      var rpcCommandRequest = new SendRpcCommandRequestComponent { TargetConnection = connectionEntity };

      ClientWorld.EntityManager.AddComponent<RpcJoinGame>(joinGameRequest);
      ClientWorld.EntityManager.AddComponentData(joinGameRequest, rpcCommandRequest);
      state.CurrentMenu = ClientMenuState.Menu.JoiningGame;

      Debug.Log("Sent request to join game");
    }
  }

  void UpdateJoiningGame(ref ClientMenuState state) {
    var networkStream = ClientWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
    var connectionEntity = ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection)).ToEntityArray(Allocator.Temp)[0];

    if (ClientWorld.EntityManager.HasComponent<NetworkStreamInGame>(connectionEntity)) {
      Unload("Main Menu");
      state.CurrentMenu = ClientMenuState.Menu.InGame;
      Debug.Log("Going InGame");
    }
  }

  void UpdateInGame(ref ClientMenuState state) {
    // TODO: For now do nothing but perhaps respond to some UI or Input events eventually?
    // Maybe the game eventually ends?
  }

  void QuitGame() {
    #if UNITY_EDITOR
    EditorApplication.isPlaying = false;
    #else
    Application.Quit(exitCode: 0);
    #endif
  }

  NetworkEndPoint NetworkEndPointForEnvironment(ushort port) {
    #if UNITY_EDITOR
    return NetworkEndPoint.Parse(ClientServerBootstrap.RequestedAutoConnect, port);
    #else
    var endPoint = NetworkEndPoint.LoopbackIpv4;

    endPoint.Port = port;
    return endPoint;
    #endif
  }

  void ConnectToServer(ref ClientMenuState state, ushort port) {
    var defaultWorld = World.DefaultGameObjectInjectionWorld;
    var clientWorldName = $"ClientWorld:{port}";
    var clientWorld = NetPongClientServerBootstrap.CreateClientWorld(defaultWorld, clientWorldName);
    var networkStream = clientWorld.GetExistingSystem<NetworkStreamReceiveSystem>();
    var endPoint = NetworkEndPointForEnvironment(port);
    var connectionEntity = networkStream.Connect(endPoint);

    UnityEngine.Debug.Log($"Client connected to server on {port}.");
    state.CurrentMenu = ClientMenuState.Menu.ConnectingToServer;
    ClientWorld = clientWorld;
  }

  void DisconnectFromServer() {
    EntityManager.AddComponent<NetworkStreamDisconnected>(GetSingletonEntity<NetworkStreamConnection>());
  }
}