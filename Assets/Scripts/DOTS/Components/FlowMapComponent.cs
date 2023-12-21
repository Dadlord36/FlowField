using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct FlowFieldContainer
    {
        public BlobArray2D<float2> flowMap;

        public static BlobAssetReference<FlowFieldContainer> CreateBlobData(in NativeArray2D<float2> inFlowField)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            
            ref FlowFieldContainer root = ref builder.ConstructRoot<FlowFieldContainer>();
            root.flowMap.Initialize(inFlowField.Width, inFlowField.Height);
            BlobBuilderArray<float2> blobArr = builder.Allocate(ref root.flowMap._array, inFlowField.Length);
            for (var i = 0; i < inFlowField.Length; i++)
            {
                blobArr[i] = inFlowField[i];
            }

            return builder.CreateBlobAssetReference<FlowFieldContainer>(Allocator.Persistent);
        }
    }

    [BurstCompile]
    public struct FlowMapComponent : ISharedComponentData
    {
        public BlobAssetReference<FlowFieldContainer> flowFieldAsset;

        public FlowMapComponent(in NativeArray2D<float2> flowField)
        {
            flowFieldAsset = FlowFieldContainer.CreateBlobData(flowField);
        }
    }
}