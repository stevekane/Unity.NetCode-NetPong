public class TitleScreen : Menu {
  public void ExitGame() {
    MenuActions.Add(MenuAction.ExitGame);
  }

  public void ExitTitleScreen() {
    MenuActions.Add(MenuAction.ExitTitleScreen);
  }
}