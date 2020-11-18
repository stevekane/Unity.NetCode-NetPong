using Unity.Entities;

[DisableAutoCreation]
public class ServerApplicationSystem : SystemBase {
  protected override void OnUpdate() {
    var serverApplication = GetSingleton<ServerApplication>();

    if (HasSingleton<SubSceneReferences>()) {
      var subSceneReferences = GetSingleton<SubSceneReferences>();

      UnityEngine.Debug.Log("Found the ssrefs");
    } else {
      UnityEngine.Debug.Log("no ssrefs");
    }
  }
}