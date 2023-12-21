using Structs;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct GridParametersComponent : ISharedComponentData
    {
        public GridParameters gridParameters;
    }
}