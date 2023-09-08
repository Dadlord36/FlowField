using DOTS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(FlowFieldMovementSystem))]
    public partial struct CylinderVelocityConvertingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
            state.RequireForUpdate<VelocityComponent>();

            // _random = new Random(214234);
        }

        // private Random _random;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (cylinderSurfacePositioningComponent, flowFieldVelocityComponent, velocityComponent)
                     in SystemAPI.Query<RefRO<CylinderSurfacePositioningComponent>, RefRO<FlowFieldVelocityComponent>, RefRW<VelocityComponent>>())
            {
                velocityComponent.ValueRW.velocity = ConvertToCylinderVelocity(flowFieldVelocityComponent.ValueRO.flowVelocity,
                    cylinderSurfacePositioningComponent.ValueRO.angle);
                /*velocityComponent.ValueRW.velocity = ConvertToCylinderVelocity(GetRandomVelocity(),
                    cylinderSurfacePositioningComponent.ValueRO.angle);*/
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        private static readonly float2 Float2One = new(1, 1);

        /*private float2 GetRandomVelocity()
        {
            return _random.NextFloat2(float2.zero, Float2One);
        }*/

        private static float3 ConvertToCylinderVelocity(in float2 flatVelocity, float angle)
        {
            // Calculate the tangent vector based on some angle (this could be dynamically determined)
            // For this example, we'll just use a placeholder angle
            var tangentVector = new float3(-math.sin(angle), 0, math.cos(angle));

            // Calculate axial and tangential components based on flatVelocity
            float3 axialComponent = flatVelocity.y * Vector3.up; // Axial movement
            float3 tangentialVelocity = tangentVector * flatVelocity.x; // Rotational movement

            // Combine them to get the final velocity
            float3 finalVelocity = axialComponent + tangentialVelocity;

            // Scale by the original speed
            float originalSpeed = math.length(flatVelocity);
            finalVelocity *= originalSpeed;

            return finalVelocity;
        }
    }
}