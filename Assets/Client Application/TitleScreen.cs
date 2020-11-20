using UnityEngine;

public class TitleScreen : Menu {
  public AudioClip TitleScreenMusic;

  public void ExitGame() {
    MenuActions.Add(MenuAction.ExitGame);
  }

  public void ExitTitleScreen() {
    MenuActions.Add(MenuAction.ExitTitleScreen);
  }
}