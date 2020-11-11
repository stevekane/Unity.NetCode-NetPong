//THIS FILE IS AUTOGENERATED BY GHOSTCOMPILER. DON'T MODIFY OR ALTER.
using Unity.Entities;
using Unity.NetCode;
using Assembly_CSharp.Generated;

namespace Assembly_CSharp.Generated
{
    [UpdateInGroup(typeof(ClientAndServerInitializationSystemGroup))]
    public class GhostComponentSerializerRegistrationSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var ghostCollectionSystem = World.GetOrCreateSystem<GhostCollectionSystem>();
            ghostCollectionSystem.AddSerializer(BallGhostComponentSerializer.State);
            ghostCollectionSystem.AddSerializer(ClaimedGhostComponentSerializer.State);
            ghostCollectionSystem.AddSerializer(PaddleGhostComponentSerializer.State);
            ghostCollectionSystem.AddSerializer(ScoresGhostComponentSerializer.State);
        }

        protected override void OnUpdate()
        {
            var parentGroup = World.GetExistingSystem<InitializationSystemGroup>();
            if (parentGroup != null)
            {
                parentGroup.RemoveSystemFromUpdateList(this);
            }
        }
    }
}