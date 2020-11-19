using Unity.Collections;
using Unity.Entities;

public struct ClientMenuState : IComponentData {
  public enum Menu { 
    Bootstrap, 
    TitleScreen, 
    MainMenu, 
    JoiningGame,
    InGame
  }

  public bool IsLoading;
  public Menu CurrentMenu;
  public Menu LastLoadingMenu;
  public FixedString128 LastLoadingMenuName;
}