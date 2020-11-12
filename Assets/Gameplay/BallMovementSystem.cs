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
  GhostPredictionSystemGroup GhostPredictionSystemGroup;
  EntityQuery PaddleQuery;

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

    Entities
    .WithName("Predicted_Ball_Movement")
    .ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, ref Rotation rotation, ref Ball ball, ref Claimed claimed, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      var oldPosition = translation.Value;
      var newPosition = oldPosition + ball.Speed * dt * forward(rotation.Value);
      var newPositionXZ = float2(newPosition.x, newPosition.z);
      var delta = newPosition - oldPosition;
      var direction = normalize(delta);
      var lengthOutsideBounds = PointOutsideCircleDistance(newPositionXZ, float2(0,0), bounds.Radius);

      // Technically, this is not adequate. You should actually solve the physics update iteratively such that a ball which 
      // deflects only to penetrate again would be solved again until it no longer penetratres.
      // This could be done by recording these motions, solving, recording motions, solving, etc
      if (lengthOutsideBounds >= 0) {
        rotation.Value = Quaternion.LookRotation(forward(rotation.Value) * float3(1,1,-1), float3(0,0,0));
        translation.Value += direction * lengthOutsideBounds + ball.Speed * dt * forward(rotation.Value);
      } else {
        translation.Value = newPosition;
      }
    })
    .WithBurst()
    .ScheduleParallel();
  }
}