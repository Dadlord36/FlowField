using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    // [DisableAutoCreation]
    public partial struct VelocityDrivenMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpeedComponent>();
            state.RequireForUpdate<VelocityComponent>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<VelocityDrivenTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (velocityComponent, localTransform, speedComponent)
                     in SystemAPI.Query<RefRO<VelocityComponent>, RefRW<LocalToWorld>, SpeedComponent>())
            {
                ref readonly LocalToWorld currentTransform = ref localTransform.ValueRO;
                float3 finalVelocity = velocityComponent.ValueRO.velocity;
                float3 newPosition = currentTransform.Position + finalVelocity * speedComponent.speed * deltaTime;

                localTransform.ValueRW.Value = float4x4.TRS(newPosition, currentTransform.Rotation, new float3(1f, 1f, 1f));
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}