using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public class ClientSampleInputToPlayerCommand : SystemBase {
  ClientSimulationSystemGroup ClientSimulationSystemGroup;

  protected override void OnCreate() {
    ClientSimulationSystemGroup = World.GetExistingSystem<ClientSimulationSystemGroup>();
  }

  protected override void OnUpdate() {
    var commandTargetEntity = GetSingletonEntity<CommandTargetComponent>();
    var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>(isReadOnly: false);
    var estimatedServerTick = ClientSimulationSystemGroup.ServerTick;
    var playerCommand = new PlayerCommand(estimatedServerTick, Input.GetAxis("Vertical"));

    Entities
    .WithName("Sample_Player_Input")
    .WithAll<Paddle>()
    .ForEach((Entity entity, DynamicBuffer<PlayerCommand> commands) => {
      commandTargetFromEntity[commandTargetEntity] = new CommandTargetComponent { targetEntity = entity };
      commands.AddCommandData(playerCommand);
    })
    .WithBurst()
    .Schedule();
  }
}