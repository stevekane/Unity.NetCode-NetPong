//THIS FILE IS AUTOGENERATED BY GHOSTCOMPILER. DON'T MODIFY OR ALTER.
using AOT;
using Unity.Burst;
using Unity.Networking.Transport;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;


namespace Assembly_CSharp.Generated
{
    [BurstCompile]
    public struct RpcsRequestLevelAckSerializer : IComponentData, IRpcCommandSerializer<RpcsRequestLevelAck>
    {
        public void Serialize(ref DataStreamWriter writer, in RpcSerializerState state, in RpcsRequestLevelAck data)
        {
            writer.WriteUInt(data.LevelGUID.Value.x);
            writer.WriteUInt(data.LevelGUID.Value.y);
            writer.WriteUInt(data.LevelGUID.Value.z);
            writer.WriteUInt(data.LevelGUID.Value.w);
        }

        public void Deserialize(ref DataStreamReader reader, in RpcDeserializerState state,  ref RpcsRequestLevelAck data)
        {
            data.LevelGUID.Value.x = (uint) reader.ReadUInt();
            data.LevelGUID.Value.y = (uint) reader.ReadUInt();
            data.LevelGUID.Value.z = (uint) reader.ReadUInt();
            data.LevelGUID.Value.w = (uint) reader.ReadUInt();
        }
        [BurstCompile]
        [MonoPInvokeCallback(typeof(RpcExecutor.ExecuteDelegate))]
        private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
        {
            RpcExecutor.ExecuteCreateRequestComponent<RpcsRequestLevelAckSerializer, RpcsRequestLevelAck>(ref parameters);
        }

        static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
            new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
        public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
        {
            return InvokeExecuteFunctionPointer;
        }
    }
    class RpcsRequestLevelAckRpcCommandRequestSystem : RpcCommandRequestSystem<RpcsRequestLevelAckSerializer, RpcsRequestLevelAck>
    {
        [BurstCompile]
        protected struct SendRpc : IJobEntityBatch
        {
            public SendRpcData data;
            public void Execute(ArchetypeChunk chunk, int orderIndex)
            {
                data.Execute(chunk, orderIndex);
            }
        }
        protected override void OnUpdate()
        {
            var sendJob = new SendRpc{data = InitJobData()};
            ScheduleJobData(sendJob);
        }
    }
}
