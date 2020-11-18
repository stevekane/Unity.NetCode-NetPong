using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class BallSpawnSystem : SystemBase {
  BeginSimulationEntityCommandBufferSystem BeginSimulationEntityCommandBufferSystem;

  protected override void OnCreate() {
    BeginSimulationEntityCommandBufferSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    RequireSingletonForUpdate<EntityPrefabs>();
  }

  protected override void OnUpdate() {
    var ecb = BeginSimulationEntityCommandBufferSystem.CreateCommandBuffer();
    var prefabs = GetSingleton<EntityPrefabs>();

    Entities
    .WithName("Spawn_Balls")
    .ForEach((ref BallSpawner spawner, in LocalToWorld localToWorld) => {
      var r = spawner.Random.NextFloat2Direction();
      var xz = float3(r.x, 0, r.y);
      var up = float3(0, 1, 0);

      spawner.TimeRemainder += spawner.SpawnsPerTick;
      while (spawner.TimeRemainder >= 1) {
        var ball = ecb.Instantiate(prefabs.Ball);

        ecb.SetComponent(ball, new Translation { Value = localToWorld.Position });
        ecb.SetComponent(ball, new Rotation { Value = Quaternion.LookRotation(xz, up) });
        spawner.TimeRemainder--;
      }
    })
    .WithBurst()
    .Schedule();
    BeginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
  }
}