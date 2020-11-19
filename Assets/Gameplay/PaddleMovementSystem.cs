using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using static PhysicsCollisionUtils;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PaddleMovementSystem : SystemBase {
  GhostPredictionSystemGroup GhostPredictionSystemGroup;

  protected override void OnCreate() {
    GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
    RequireSingletonForUpdate<GameConfiguration>();
  }

  protected override void OnUpdate() {
    var dt = Time.DeltaTime;
    var predictingTick = GhostPredictionSystemGroup.PredictingTick;
    var gameConfig = GetSingleton<GameConfiguration>();

    Entities
    .ForEach((ref Translation translation, ref Rotation rotation, ref Paddle paddle, in DynamicBuffer<PlayerCommand> commands, in GhostOwnerComponent ghostOwner, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      if (!commands.GetDataAtTick(predictingTick, out PlayerCommand command)) {
        return;
      }

      if (command.Pushed(PlayerCommand.Up)) {
        paddle.Radians = AddRadians(paddle.Radians, gameConfig.PaddleSpeed * dt);
      } else if (command.Pushed(PlayerCommand.Down)) {
        paddle.Radians = AddRadians(paddle.Radians, -gameConfig.PaddleSpeed * dt);
      }
      var up = float3(0, 1, 0);
      var x = cos(paddle.Radians) * gameConfig.ArenaRadius;
      var z = sin(paddle.Radians) * gameConfig.ArenaRadius;
      var forward = normalize(float3(-x, 0, -z));

      rotation.Value = Quaternion.LookRotation(forward, up);
      translation.Value = float3(x, 0, z);
    })
    .WithBurst()
    .ScheduleParallel();
  }
}