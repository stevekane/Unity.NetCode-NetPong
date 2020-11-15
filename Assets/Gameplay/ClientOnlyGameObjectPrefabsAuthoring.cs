using Unity.Entities;
using UnityEngine;

public class ClientOnlyGameObjectPrefabsAuthoring : MonoBehaviour {
  public static ClientOnlyGameObjectPrefabsAuthoring Instance;

  public GameObject ScoreGUIPrefab;

  public void Awake() {
    Instance = this;
  }
}