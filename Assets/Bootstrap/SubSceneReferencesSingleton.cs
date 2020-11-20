using Unity.Scenes;
using UnityEngine;

public class SubSceneReferencesSingleton : MonoBehaviour {
  public static SubSceneReferencesSingleton Instance;

  public SubScene SharedResources;
  public SubScene StaticGeometry;
  public SubScene Ghosts;
  public SubScene GameState;
  public SubScene ClientSingletons;

  public SubSceneReferencesSingleton CreateInstance() {
    Instance = this;
    return this;
  }
}