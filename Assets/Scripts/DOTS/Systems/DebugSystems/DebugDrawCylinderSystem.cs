using DOTS.Components;
using DOTS.Components.Tags;
using DOTS.Jobs;
using Drawing;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    public partial struct DebugDrawCylinderSystem : ISystem
    {
        private static readonly float3 Up = Vector3.up;

        [BurstCompile]
        [WithAll(typeof(WalkingSurfaceTag))]
        private partial struct DrawCylinderJob : IJobEntity
        {
            private CommandBuilder _drawingBuilder;

            public DrawCylinderJob(in CommandBuilder drawingBuilder) : this()
            {
                _drawingBuilder = drawingBuilder;
            }

            private void Execute(in CylinderParametersComponent cylinderParametersComponent)
            {
                ref readonly CylinderParameters cylinderParameters = ref cylinderParametersComponent.cylinderParameters;
                _drawingBuilder.WireCylinder(cylinderParameters.cylinderOrigin, Up, cylinderParameters.radius,
                    cylinderParameters.height, Color.magenta);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CylinderParametersComponent>();
            // state.RequireForUpdate<GridParametersComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);
            /*var gridParametersComponent = SystemAPI.GetSingleton<GridParametersComponent>();
            ref GridParameters gridParameters = ref gridParametersComponent.gridParameters;*/

            state.Dependency = new DrawCylinderJob(drawingBuilder).Schedule( state.Dependency);
            /*JobHandle drawCellsOnCylinderJobHandle =
                new DrawOccupiedCellOnCylinderJob(drawingBuilder, gridParameters, cylinderParametersComponent.cylinderParameters).ScheduleParallel(
                    drawCylinderJobHandle);*/

            drawingBuilder.DisposeAfter(state.Dependency);

            // state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}