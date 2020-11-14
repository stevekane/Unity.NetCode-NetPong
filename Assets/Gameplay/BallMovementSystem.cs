using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Jobs;
using static Unity.Mathematics.math;

[UpdateInWorld(UpdateInWorld.TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class BallMovementSystem : SystemBase {
  protected override void OnUpdate() {
    var dt = Time.DeltaTime;
    var gameConfig = GetSingleton<GameConfiguration>();

    Entities
    .WithName("Predicted_Ball_Movement")
    .ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, in Rotation rotation, in Ball ball) => {
      translation.Value += gameConfig.BallSpeed * dt * forward(rotation.Value);
    })
    .WithBurst()
    .ScheduleParallel();
  }
}