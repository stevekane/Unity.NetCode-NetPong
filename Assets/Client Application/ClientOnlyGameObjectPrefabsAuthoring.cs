using UnityEngine;

public class ClientOnlyGameObjectPrefabsAuthoring : MonoBehaviour {
  public static ClientOnlyGameObjectPrefabsAuthoring Instance;

  public GameObject ScoreGUIPrefab;

  public ClientOnlyGameObjectPrefabsAuthoring CreateInstance() {
    Instance = this;
    return this;
  }
}