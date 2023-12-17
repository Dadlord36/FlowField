using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct CellIndexComponent : IComponentData
    {
        public static readonly CellIndexComponent Empty = new CellIndexComponent { index = uint2.zero };
        public uint2 index;

        public CellIndexComponent(uint2 index)
        {
            this.index = index;
        }
    }
}