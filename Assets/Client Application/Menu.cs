using System.Collections.Generic;
using UnityEngine;

public abstract class Menu : MonoBehaviour {
  protected List<MenuAction> MenuActions;

  public void RegisterMenuActions(List<MenuAction> menuActions) {
    MenuActions = menuActions;
  }
}