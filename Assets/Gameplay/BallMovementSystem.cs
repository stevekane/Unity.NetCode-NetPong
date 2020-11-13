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
    var isServer = World.GetExistingSystem<ServerSimulationSystemGroup>() != null;
    var predictingTick = GhostPredictionSystemGroup.PredictingTick;
    var isFinalTick = GhostPredictionSystemGroup.IsFinalPredictionTick;
    var dt = Time.DeltaTime;
    var bounds = GetSingleton<Bounds>();

    Entities
    .WithName("Predicted_Ball_Movement")
    .ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, ref Rotation rotation, ref Ball ball, ref Claimed claimed, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      const float PaddleRadius = 15f;
      const float PaddleRadians = PI / 4f;
      const float PaddleMinRadians = PaddleRadians - .2f;
      const float PaddleMaxRadians = PaddleRadians + .2f;

      var originXZ = float2(0,0);
      var oldPosition = translation.Value;
      var newPosition = oldPosition + ball.Speed * dt * forward(rotation.Value);
      var newPositionXZ = float2(newPosition.x, newPosition.z);
      var delta = newPosition - oldPosition;
      var direction = normalize(delta);
      var lengthOutsideBoundsRadius = PointOutsideCircleDistance(newPositionXZ, originXZ, bounds.Radius);
      var lengthOutsidePaddleRadius = PointOutsideCircleDistance(newPositionXZ, originXZ, PaddleRadius);

      if (lengthOutsideBoundsRadius > 0) {
        var totalDistance = length(delta);
        var distanceToContact = totalDistance - lengthOutsideBoundsRadius;

        translation.Value += distanceToContact * forward(rotation.Value);
        rotation.Value = Quaternion.LookRotation(forward(rotation.Value) * float3(-1,1,-1), float3(0,1,0));
        translation.Value += lengthOutsideBoundsRadius * forward(rotation.Value);
      } else if (lengthOutsidePaddleRadius > 0) {
        var totalDistance = length(delta);
        var distanceToContact = totalDistance - lengthOutsidePaddleRadius;
        var contactPosition = oldPosition + forward(rotation.Value) * distanceToContact;
        var contactPostionXZ = new float2(contactPosition.x, contactPosition.z);
        var contactRadians = atan2(contactPosition.z, contactPosition.x);

        if (WithinArcSegment(contactRadians, PaddleMinRadians, PaddleMaxRadians)) {
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
    .WithBurst()
    .ScheduleParallel();
  }
}