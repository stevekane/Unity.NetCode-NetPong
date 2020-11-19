using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class ClientHandleRpcSystem : SystemBase {
  public static Entity CreateRpc(EntityManager entityManager, Entity targetConnection) {
    var entity = entityManager.CreateEntity();
    var sendRpc = new SendRpcCommandRequestComponent { TargetConnection = targetConnection };

    entityManager.AddComponentData(entity, sendRpc);
    return entity;
  }

  protected override void OnCreate() {
    RequireSingletonForUpdate<NetworkIdComponent>();
  }

  protected override void OnUpdate() {
    var networkId = GetSingleton<NetworkIdComponent>().Value;
    var connectionEntity = GetSingletonEntity<NetworkStreamConnection>();

    Entities
    .WithName("Handle_RequestLevelAck")
    .ForEach((Entity requestEntity, in RpcsRequestLevelAck ack, in ReceiveRpcCommandRequestComponent request) => {
      ClientConnectionSystem.CreateConnectionEvent(EntityManager, new ClientConnectionEvent {
        EventName = ClientConnectionEvent.Name.LoadLevel,
        LevelGUID = ack.LevelGUID
      });
      EntityManager.DestroyEntity(requestEntity);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();

    Entities
    .WithName("Handle_JoinGameAck")
    .ForEach((Entity requestEntity, in RpcJoinGameAck ack, in ReceiveRpcCommandRequestComponent request) => {
      ClientConnectionSystem.CreateConnectionEvent(EntityManager, new ClientConnectionEvent {
        EventName = ClientConnectionEvent.Name.JoinGameAcknowledged
      });
      EntityManager.DestroyEntity(requestEntity);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
  }
}