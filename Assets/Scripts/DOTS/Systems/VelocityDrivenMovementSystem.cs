using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VelocityDrivenMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpeedComponent>();
            state.RequireForUpdate<VelocityComponent>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<VelocityDrivenTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (velocityComponent, flowFieldVelocityComponent, localTransform, speedComponent)
                     in SystemAPI.Query<RefRO<VelocityComponent>, RefRO<FlowFieldVelocityComponent>, RefRW<LocalToWorld>, SpeedComponent>())
            {
                float2 flowFieldVelocity = flowFieldVelocityComponent.ValueRO.flowVelocity;
                var finalVelocity = new float3(flowFieldVelocity.x, 0f, flowFieldVelocity.y);
                // velocityComponent.ValueRW.velocity = math.normalize(velocityComponent.ValueRO.velocity) * speedComponent.speed;
                float3 newPosition = localTransform.ValueRW.Position + finalVelocity * speedComponent.speed * deltaTime;
                localTransform.ValueRW.Value = float4x4.Translate(newPosition);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}