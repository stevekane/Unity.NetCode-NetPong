using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
[UpdateAfter(typeof(PaddleMovementSystem))]
public class BallMovementSystem : SystemBase {
  GhostPredictionSystemGroup GhostPredictionSystemGroup;
  EntityQuery PaddleQuery;

  public static bool PenetratesWalls(in float3 newPosition, in Bounds bounds) {
    return (newPosition.z < bounds.Min.z) || (newPosition.z > bounds.Max.z);
  }

  public static bool PenetrateLeftGoalLine(in float3 newPosition, in Bounds bounds) {
    return newPosition.x < bounds.Min.x;
  }

  public static bool PenetrateRightGoalLine(in float3 newPosition, in Bounds bounds) {
    return newPosition.x > bounds.Max.x;
  }

  public static bool OverlapBox(in float3 position, in float3 dimensions, in float3 center) {
    var min = center - dimensions / 2;
    var max = center + dimensions / 2;

    return (position.x <= max.x && position.x >= min.x) 
        && (position.y <= max.y && position.y >= min.y)
        && (position.z <= max.z && position.z >= min.z);
  }

  // n^2 huzzah!
  public static bool PenetratesAnyPaddle(
  in float3 oldPosition, 
  in float3 newPosition, 
  NativeArray<Paddle> paddles, 
  NativeArray<Translation> paddleTranslations) {
    for (var i = 0 ; i < paddles.Length; i++) {
      if (OverlapBox(oldPosition, paddles[i].Dimensions, paddleTranslations[i].Value)) {
        return true;
      }
      if (OverlapBox(newPosition, paddles[i].Dimensions, paddleTranslations[i].Value)) {
        return true;
      }
    }
    return false;
  }

  protected override void OnCreate() {
    GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
    PaddleQuery = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<Paddle>(),
      ComponentType.ReadOnly<Translation>()
    });
  }

  protected override void OnUpdate() {
    var predictingTick = GhostPredictionSystemGroup.PredictingTick;
    var dt = Time.DeltaTime;
    var bounds = GetSingleton<Bounds>();
    var paddles = PaddleQuery.ToComponentDataArray<Paddle>(Allocator.TempJob);
    var paddleTranslations = PaddleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

    Entities
    .WithName("Predicted_Ball_Movement")
    .ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, ref Rotation rotation, ref Ball ball, ref Claimed claimed, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      var oldPosition = translation.Value;
      var newPosition = oldPosition + ball.Speed * dt * forward(rotation.Value);

      if (PenetrateLeftGoalLine(newPosition, bounds)) {
        claimed.TeamIndex = 1;
      } else if (PenetrateRightGoalLine(newPosition, bounds)) {
        claimed.TeamIndex = 2;
      } else if (PenetratesWalls(newPosition, bounds)) {
        rotation.Value = Quaternion.LookRotation(forward(rotation.Value) * float3(1,1,-1), float3(0,0,0));
      } else if (PenetratesAnyPaddle(oldPosition, newPosition, paddles, paddleTranslations)) {
        rotation.Value = Quaternion.LookRotation(forward(rotation.Value) * float3(-1,1,1), float3(0,0,0));
      }
      translation.Value += ball.Speed * dt * forward(rotation.Value);
    })
    .WithReadOnly(paddles)
    .WithReadOnly(paddleTranslations)
    .WithDisposeOnCompletion(paddles)
    .WithDisposeOnCompletion(paddleTranslations)
    .WithBurst()
    .ScheduleParallel();
  }
}