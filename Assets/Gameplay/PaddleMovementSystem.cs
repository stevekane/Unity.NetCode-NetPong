using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PaddleMovementSystem : SystemBase {
  GhostPredictionSystemGroup GhostPredictionSystemGroup;

  public struct Hit {
    public bool HitMax;
    public float3 ContactPointMax;
    public bool HitMin;
    public float3 ContactPointMin;
  }

  public static bool Equal(in float3 a, in float3 b) {
    return a.x == b.x && a.y == b.y && a.z == b.z;
  }

  public static bool PenetratesWalls(
  in float3 p1,
  in float3 dimensions,
  in Bounds bounds,
  out Hit hit) {
    var zero = float3(0,0,0);
    var halfHeight = dimensions / 2f;
    var deltaMax = (p1 + halfHeight) - bounds.Max;
    var deltaMin = (p1 - halfHeight) - bounds.Min;
    var penetrationMax = max(deltaMax, zero);
    var penetrationMin = min(deltaMin, zero);

    hit.HitMax = !Equal(penetrationMax, zero);
    hit.ContactPointMax = p1 - penetrationMax;
    hit.HitMin = !Equal(penetrationMin, zero);
    hit.ContactPointMin = p1 - penetrationMin;
    return hit.HitMax || hit.HitMin;
  }

  protected override void OnCreate() {
    GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
  }

  protected override void OnUpdate() {
    var dt = Time.DeltaTime;
    var predictingTick = GhostPredictionSystemGroup.PredictingTick;
    var bounds = GetSingleton<Bounds>();

    Entities
    .ForEach((ref Translation translation, in Paddle paddle, in DynamicBuffer<PlayerCommand> commands, in PredictedGhostComponent predictedGhost) => {
      if (!GhostPredictionSystemGroup.ShouldPredict(predictingTick, predictedGhost)) {
        return;
      }

      if (!commands.GetDataAtTick(predictingTick, out PlayerCommand command)) {
        return;
      }

      if (command.Pushed(PlayerCommand.Up)) {
        translation.Value.z += dt * paddle.Speed;
      } else if (command.Pushed(PlayerCommand.Down)) {
        translation.Value.z -= dt * paddle.Speed;
      }

      if (PenetratesWalls(translation.Value, paddle.Dimensions, bounds, out Hit hit)) {
        if (hit.HitMax && hit.HitMin) {
          translation.Value = (hit.ContactPointMax - hit.ContactPointMin) / 2f;
        } else if (hit.HitMax) {
          translation.Value = hit.ContactPointMax;
        } else if (hit.HitMin) {
          translation.Value = hit.ContactPointMin;
        }
      }
    })
    .WithBurst()
    .ScheduleParallel();
  }
}