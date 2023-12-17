using Unity.Burst;
using Unity.Entities;

namespace DOTS.Buffers
{
    [BurstCompile]
    [InternalBufferCapacity(1)]
    public struct BlockingEntityElement : IBufferElementData
    {
        public Entity entity;

        public BlockingEntityElement(Entity entity)
        {
            this.entity = entity;
        }
    }
}