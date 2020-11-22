using UnityEngine;

public class SubSceneReferencesSingleton : MonoBehaviour {
  public static SubSceneReferencesSingleton Instance;

  public Board MainBoard;

  public SubSceneReferencesSingleton CreateInstance() {
    Instance = this;
    return this;
  }
}