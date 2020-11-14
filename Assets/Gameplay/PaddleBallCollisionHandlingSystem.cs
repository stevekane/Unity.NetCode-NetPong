using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using static PhysicsCollisionUtils;

[UpdateInWorld(UpdateInWorld.TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PaddleBallCollisionHandlingSystem : SystemBase {
  EntityQuery PaddleQuery;

  protected override void OnCreate() {
    PaddleQuery = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<Paddle>(),
      ComponentType.ReadOnly<TeamOwner>(),
      ComponentType.ReadOnly<CommandDataInterpolationDelay>()
    });
  }

  protected override void OnUpdate() {
    var dt = Time.DeltaTime;
    var paddles = PaddleQuery.ToComponentDataArray<Paddle>(Allocator.TempJob);
    var paddleTeamOwners = PaddleQuery.ToComponentDataArray<TeamOwner>(Allocator.TempJob);
    var interpolationDelays = PaddleQuery.ToComponentDataArray<CommandDataInterpolationDelay>(Allocator.TempJob);
    var gameConfig = GetSingleton<GameConfiguration>();
    var collisionRadius = gameConfig.ArenaRadius - 1f;

    Entities
    .ForEach((Entity ballEntity, ref Translation translation, ref Rotation rotation, ref Ball ball, ref TeamOwner ballTeamOwner, ref LifeCycle lifeCycle) => {
      var oldPositionXZ = FromXZPlane(translation.Value - dt * gameConfig.BallSpeed * forward(rotation.Value));
      var newPositionXZ = FromXZPlane(translation.Value);
      var enteredPaddleRadius = LineSegmentCircleIntersectionAPPROXIMATE(oldPositionXZ, newPositionXZ, collisionRadius, out float2 contactPoint);

      if (enteredPaddleRadius) {
        var totalDistance = length(newPositionXZ - oldPositionXZ);
        var distanceToContact = distance(contactPoint, oldPositionXZ);
        var distanceOutsidePaddleRadius = totalDistance - distanceToContact;
        var contactRadians = CartesianToRadians(contactPoint);

        lifeCycle.CurrentState = LifeCycle.State.Dead;
        for (int i = 0; i < paddles.Length; i++) {
          var delay = interpolationDelays[i].Delay;
          var minRadians = paddles[i].Radians;
          var maxRadians = AddRadians(paddles[i].Radians, gameConfig.PaddleSpanRadians);
          var paddleTeamOwner = paddleTeamOwners[i];

          if (WithinArcSegment(contactRadians, minRadians, maxRadians)) {
            var up = float3(0, 1, 0);
            var bounceDirection = ToXZPlane(ReflectAbout(normalize(newPositionXZ - oldPositionXZ), -contactPoint));

            lifeCycle.CurrentState = LifeCycle.State.Alive;
            ballTeamOwner.TeamIndex = paddleTeamOwners[i].TeamIndex;
            translation.Value = ToXZPlane(contactPoint);
            rotation.Value = Quaternion.LookRotation(bounceDirection, float3(0,1,0));
            translation.Value += distanceOutsidePaddleRadius * forward(rotation.Value);
            break;
          }
        }
      }
    })
    .WithReadOnly(paddles)
    .WithReadOnly(paddleTeamOwners)
    .WithReadOnly(interpolationDelays)
    .WithDisposeOnCompletion(paddles)
    .WithDisposeOnCompletion(paddleTeamOwners)
    .WithDisposeOnCompletion(interpolationDelays)
    .WithBurst()
    .ScheduleParallel();
  }
}