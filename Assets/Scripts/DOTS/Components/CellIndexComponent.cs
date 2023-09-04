using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct CellIndexComponent : IComponentData
    {
        public static readonly CellIndexComponent Invalid = new CellIndexComponent {index = new int2(-1, -1)};
        public int2 index;

        public CellIndexComponent(int2 index)
        {
            this.index = index;
        }

        public bool IsValid => index.x >= 0 && index.y >= 0;
    }
}