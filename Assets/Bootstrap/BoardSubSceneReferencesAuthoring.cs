using System;
using Unity.Scenes;
using UnityEngine;

public struct BoardSubSceneReferences {
  public Hash128 GameState;
  public Hash128 Ghosts;
  public Hash128 StaticGeometry;

  public BoardSubSceneReferences(in BoardSceneReferencesAuthoring authoring) {
    GameState = authoring.GameState.SceneGUID;
    Ghosts = authoring.Ghosts.SceneGUID;
    StaticGeometry = authoring.StaticGeometry.SceneGUID;
  }
}

[Serializable]
public struct BoardSceneReferencesAuthoring {
  public SubScene GameState;
  public SubScene Ghosts;
  public SubScene StaticGeometry;
}
