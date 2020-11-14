using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostPredictionSystemGroup), OrderFirst=true)]
public class PaddleHistorySystem : SystemBase {
  /*
  const int MAX_PADDLES_PER_TICK = 64;
  const int MAX_TICKS_STORED_ON_SERVER = 16;
  const int MAX_TICKS_STORED_ON_CLIENT = 1;

  public NativeArray<Paddle> PaddleHistory;
  public NativeArray<Entity> PaddleHistoryEntities;
  public NativeArray<int> Counts;
  public int LatestIndex;
  public JobHandle StoreHistoryJob;
  public JobHandle FinalHistoryJob;

  EntityQuery PaddleQuery;
  int MaxTicks;
  bool IsServer;

  protected override void OnCreate() {
    IsServer = World.GetExistingSystem<ServerSimulationSystemGroup>() != null;
    PaddleQuery = EntityManager.CreateEntityQuery(new ComponentType[] {
      ComponentType.ReadOnly<Paddle>(),
      ComponentType.ReadOnly<Translation>()
    });
    if (IsServer) {
      var totalSize = MAX_PADDLES_PER_TICK * MAX_TICKS_STORED_ON_SERVER;

      MaxTicks = MAX_TICKS_STORED_ON_SERVER;
      PaddleHistory = new NativeArray<Paddle>(totalSize, Allocator.Persistent);
      PaddleHistoryEntities = new NativeArray<Entity>(totalSize, Allocator.Persistent);
      Counts = new NativeArray<int>(totalSize, Allocator.Persistent);
      LatestIndex = -1;
    } else {
      var totalSize = MAX_PADDLES_PER_TICK * MAX_TICKS_STORED_ON_CLIENT;

      MaxTicks = MAX_TICKS_STORED_ON_CLIENT;
      PaddleHistory = new NativeArray<Paddle>(totalSize, Allocator.Persistent);
      PaddleHistoryEntities = new NativeArray<Entity>(totalSize, Allocator.Persistent);
      Counts = new NativeArray<int>(totalSize, Allocator.Persistent);
      LatestIndex = 0;
    }
  }
  */

  protected override void OnUpdate() {
    /*
    LatestIndex = (LatestIndex + 1 >= MaxTicks) ? (0) : (LatestIndex + 1);

    var targetIndex = LatestIndex * MAX_PADDLES_PER_TICK;
    var paddleHistory = PaddleHistory;
    var paddles = PaddleQuery.ToComponentDataArray<Paddle>(Allocator.TempJob);
    var counts = Counts;

    StoreHistoryJob = Job
    .WithCode(() => {
      counts[targetIndex] = paddles.Length;
      NativeArray<Paddle>.Copy(paddles, 0, paddleHistory, targetIndex, paddles.Length);
    })
    .WithBurst()
    .Schedule(JobHandle.CombineDependencies(Dependency, FinalHistoryJob));
    */
  }
}