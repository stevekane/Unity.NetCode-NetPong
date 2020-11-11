public class MainMenu : Menu {
  public void ExitGame() {
    MenuActions.Add(MenuAction.ExitGame);
  }

  public void JoinGame() {
    MenuActions.Add(MenuAction.JoinGame);
  }
}