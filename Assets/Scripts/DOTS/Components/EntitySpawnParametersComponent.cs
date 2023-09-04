using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct EntitySpawnParametersComponent : IComponentData
    {
        public int entityCount;
        public float speed;
    }
}