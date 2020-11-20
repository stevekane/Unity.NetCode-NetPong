using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public class RenderScoresSystem : SystemBase {
  class ScoreOverlayInstance: ISystemStateComponentData {
    public ScoreOverlay ScoreOverlay;
  }

  protected override void OnCreate() {
    Debug.Log($"ALERT. RENDERSCORESSYSTEM IS CURRENTLY DISABLED");
  }

  protected override void OnUpdate() {
    // var scoreOverlayPrefab = ClientOnlyGameObjectPrefabsAuthoring.Instance.ScoreGUIPrefab.GetComponent<ScoreOverlay>();

    // Entities
    // .WithName("Create_ScoreGUI")
    // .WithNone<ScoreOverlayInstance>()
    // .WithAll<Scores>()
    // .ForEach((Entity e) => {
    //   EntityManager.AddComponentData(e, new ScoreOverlayInstance {
    //     ScoreOverlay = ScoreOverlay.Instantiate(scoreOverlayPrefab)
    //   });
    // })
    // .WithStructuralChanges()
    // .WithoutBurst()
    // .Run();

    // Entities
    // .WithName("Destroy_Unneeded_ScoreGUI")
    // .WithNone<Scores>()
    // .WithAll<ScoreOverlayInstance>()
    // .ForEach((Entity e, ScoreOverlayInstance instance) => {
    //   GameObject.Destroy(instance.ScoreOverlay.gameObject);
    //   EntityManager.RemoveComponent<ScoreOverlayInstance>(e);
    // })
    // .WithStructuralChanges()
    // .WithoutBurst()
    // .Run();

    // Entities
    // .WithName("Update_ScoreGUI")
    // .ForEach((Entity entity, ScoreOverlayInstance instance, in Scores scores) => {
    //   instance.ScoreOverlay.LeftScore.text = scores.LeftTeam.ToString();
    //   instance.ScoreOverlay.RightScore.text = scores.RightTeam.ToString();
    // })
    // .WithoutBurst()
    // .Run();
  }
}