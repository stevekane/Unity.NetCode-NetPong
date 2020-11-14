using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class KillDeadBallsSystem : SystemBase {
  protected override void OnUpdate() {
    // TODO: would probably be a bit better to do this with a query for balls
    // and a single write to the score followed by a second job that writes 
    // destroys into an entity command buffer.

    Entities
    .WithName("Record_Score_For_Dead_Balls")
    .WithAll<Ball>()
    .ForEach((Entity e, in LifeCycle lifeCycle, in TeamOwner teamOwner) => {
      if (lifeCycle.CurrentState != LifeCycle.State.Dead) {
        return;
      }

      var scoreEntity = GetSingletonEntity<Scores>();
      var score = GetComponent<Scores>(scoreEntity);

      switch (teamOwner.TeamIndex) {
      case 0:
        score.LeftTeam++;
        SetComponent(scoreEntity, score);
      break;

      case 1:
        score.RightTeam++;
        SetComponent(scoreEntity, score);
      break;
      }
    })
    .WithoutBurst()
    .Run();

    Entities
    .WithName("Kill_The_Dead")
    .ForEach((Entity e, in LifeCycle lifeCycle) => {
      if (lifeCycle.CurrentState == LifeCycle.State.Dead) {
        EntityManager.DestroyEntity(e);
      }
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
  }
}