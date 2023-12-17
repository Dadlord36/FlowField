using DOTS.Components;
using DOTS.Components.Tags;
using Drawing;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    public partial struct DebugDrawForwardArrowSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VelocityDrivenTag>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<VelocityComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder forwardArrowsBuilder = DrawingManager.GetBuilder(true);
            CommandBuilder velocityArrowsBuilder = DrawingManager.GetBuilder(true);

            foreach (var (localTransform, velocityComponent) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<VelocityComponent>>())
            {
                ref readonly LocalToWorld currentTransform = ref localTransform.ValueRO;

                float3 forwardArrowTipPosition = currentTransform.Position + currentTransform.Forward;
                forwardArrowsBuilder.Arrow(currentTransform.Position, forwardArrowTipPosition, Color.blue);

                float3 velocityArrowTipPosition = currentTransform.Position + velocityComponent.ValueRO.velocity;
                velocityArrowsBuilder.Arrow(currentTransform.Position, velocityArrowTipPosition, Color.red);
            }

            forwardArrowsBuilder.Dispose();
            velocityArrowsBuilder.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}