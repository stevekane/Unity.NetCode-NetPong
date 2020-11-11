using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class KillDeadBallsSystem : SystemBase {
  protected override void OnUpdate() {
    // This is fundamentally an accumulator so let's just run it on the main thread
    // I suppose it could be a job that just writes everytime to the scores as well
    Entities
    .WithName("Kill_Dead_Balls")
    .WithAll<Ball>()
    .ForEach((Entity e, in Claimed claimed) => {
      var scoreEntity = GetSingletonEntity<Scores>();
      var score = GetComponent<Scores>(scoreEntity);

      switch (claimed.TeamIndex) {
      case 1:
        score.LeftTeam++;
        SetComponent(scoreEntity, score);
        EntityManager.DestroyEntity(e);
      break;

      case 2:
        score.RightTeam++;
        SetComponent(scoreEntity, score);
        EntityManager.DestroyEntity(e);
      break;
      }
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
  }
}