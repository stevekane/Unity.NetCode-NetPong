using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using static Unity.Mathematics.math;
using static PhysicsCollisionUtils;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
[UpdateAfter(typeof(PaddleMovementSystem))]
public class BallMovementSystem : SystemBase {
  PaddleHistorySystem PaddleHistorySystem;
  GhostPredictionSystemGroup GhostPredictionSystemGroup;

  protected override void OnCreate() {
    PaddleHistorySystem = World.GetExistingSystem<PaddleHistorySystem>();
    GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
  }

  protected override void OnUpdate() {
    var isServer = World.GetExistingSystem<ServerSimulationSystemGroup>() != null;
    var predictingTick = GhostPredictionSystemGroup.PredictingTick;
    var isFinalTick = GhostPredictionSystemGroup.IsFinalPredictionTick;
    var dt = Time.DeltaTime;
    var gameConfig = GetSingleton<GameConfiguration>();
    var collisionRadius = gameConfig.ArenaRadius - 1f;
    var paddleHistory = PaddleHistorySystem.PaddleHistory;
    var paddleHistoryCounts = PaddleHistorySystem.Counts;
    var paddleHistoryIndex = PaddleHistorySystem.LatestIndex;

    Dependency = Entities
    .WithName("Predicted_Ball_Movement")
    .ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, ref Rotation rotation, ref Ball ball, ref Claimed claimed, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      var paddles = paddleHistory.Slice(paddleHistoryIndex, paddleHistoryCounts[paddleHistoryIndex]);
      var originXZ = float2(0,0);
      var oldPosition = translation.Value;
      var newPosition = oldPosition + gameConfig.BallSpeed * dt * forward(rotation.Value);
      var newPositionXZ = float2(newPosition.x, newPosition.z);
      var delta = newPosition - oldPosition;
      var lengthOutsidePaddleRadius = PointOutsideCircleDistance(newPositionXZ, originXZ, collisionRadius);

      if (lengthOutsidePaddleRadius > 0) {
        var totalDistance = length(delta);
        var distanceToContact = totalDistance - lengthOutsidePaddleRadius;
        var contactPosition = oldPosition + forward(rotation.Value) * distanceToContact;
        var contactPostionXZ = new float2(contactPosition.x, contactPosition.z);
        var contactRadians = CartesianToRadians(contactPostionXZ);
        var hitPaddle = false;

        for (int i = 0; i < paddles.Length; i++) {
          var minRadians = paddles[i].Radians;
          var maxRadians = AddRadians(paddles[i].Radians, gameConfig.PaddleSpanRadians);

          if (WithinArcSegment(contactRadians, minRadians, maxRadians)) {
            hitPaddle = true;
          }
        }

        if (hitPaddle) {
          translation.Value += distanceToContact * forward(rotation.Value);
          rotation.Value = Quaternion.LookRotation(forward(rotation.Value) * float3(-1,1,-1), float3(0,1,0));
          translation.Value += lengthOutsidePaddleRadius * forward(rotation.Value);
        } else {
          translation.Value = newPosition;
        }
      } else {
        translation.Value = newPosition;
      }
    })
    .WithReadOnly(paddleHistory)
    .WithReadOnly(paddleHistoryCounts)
    .WithBurst()
    .Schedule(JobHandle.CombineDependencies(PaddleHistorySystem.StoreHistoryJob, Dependency));
    PaddleHistorySystem.FinalHistoryJob = Dependency;
  }
}