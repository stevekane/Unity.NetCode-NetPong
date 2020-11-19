#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum MenuAction { 
  ExitTitleScreen,
  ExitGame,
  JoinGame
}

[DisableAutoCreation]
public class ClientMenuSystem : SystemBase {
  public static List<MenuAction> MenuActions = new List<MenuAction>(16);

  public World ClientWorld;

  public static bool TryGetSingletonEntityFromOtherWorld<T>(World world, out Entity entity) where T : IComponentData {
    var entities = world.EntityManager.CreateEntityQuery(typeof(T)).ToEntityArray(Allocator.Temp);

    if (entities.Length == 1) {
      entity = entities[0];
      return true;
    } else {
      entity = Entity.Null;
      return false;
    }
  }

  public static bool TryGetSingletonFromOtherWorld<T>(World world, out T t) where T : struct, IComponentData {
    var results = world.EntityManager.CreateEntityQuery(typeof(T)).ToComponentDataArray<T>(Allocator.Temp);

    if (results.Length == 1) {
      t = results[0];
      return true;
    } else {
      t = default(T);
      return false;
    }
  }

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

      case ClientMenuState.Menu.JoiningGame:
        CheckIfInGame(ref state);
      break;

      case ClientMenuState.Menu.InGame:
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
        InitiateJoinGame(ref state, NetPongClientServerBootstrap.DEFAULT_PORT);
      break;

      case MenuAction.ExitGame:
        Debug.Log("You quit!");
        QuitGame();
      break;
      }
    }
  }

  void CheckIfInGame(ref ClientMenuState state) {
    if (TryGetSingletonFromOtherWorld(ClientWorld, out ClientConnection connection)) {
      if (connection.CurrentState == ClientConnection.State.InGame) {
        Unload("Main Menu");
        state.CurrentMenu = ClientMenuState.Menu.InGame;
      }
    }
  }

  void QuitGame() {
    #if UNITY_EDITOR
    EditorApplication.isPlaying = false;
    #else
    Application.Quit(exitCode: 0);
    #endif
  }

  void InitiateJoinGame(ref ClientMenuState state, ushort port) {
    ClientConnectionSystem.CreateConnectionEvent(ClientWorld.EntityManager, new ClientConnectionEvent {
      EventName = ClientConnectionEvent.Name.Connect,
      Port = port
    });
    UnityEngine.Debug.Log($"Client initiated join game on port {port}.");
    state.CurrentMenu = ClientMenuState.Menu.JoiningGame;
  }

  void DisconnectFromServer() {
    EntityManager.AddComponent<NetworkStreamDisconnected>(GetSingletonEntity<NetworkStreamConnection>());
  }
}