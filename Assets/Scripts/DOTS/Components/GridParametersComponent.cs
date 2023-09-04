using Structs;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct GridParametersComponent : IComponentData
    {
        public GridParameters gridParameters;
    }
}