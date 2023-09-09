using DOTS.Components;
using DOTS.Jobs;
using Drawing;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;


namespace DOTS.Systems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    public partial struct DebugDrawCylinderSystem : ISystem
    {
        private static readonly float3 Up = Vector3.up;

        [BurstCompile]
        private struct DrawCylinderJob : IJob
        {
            private readonly CylinderParameters _cylinderParameters;
            private CommandBuilder _drawingBuilder;

            public DrawCylinderJob(in CylinderParameters cylinderParameters, in CommandBuilder drawingBuilder)
            {
                _cylinderParameters = cylinderParameters;
                _drawingBuilder = drawingBuilder;
            }

            public void Execute()
            {
                _drawingBuilder.WireCylinder(_cylinderParameters.cylinderOrigin, Up, _cylinderParameters.radius,
                    _cylinderParameters.height, Color.magenta);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CylinderParametersComponent>();
            state.RequireForUpdate<GridParametersComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);
            var cylinderParametersComponent = SystemAPI.GetSingleton<CylinderParametersComponent>();
            var gridParametersComponent = SystemAPI.GetSingleton<GridParametersComponent>();
            ref GridParameters gridParameters = ref gridParametersComponent.gridParameters;

            JobHandle drawCylinderJobHandle =
                new DrawCylinderJob(cylinderParametersComponent.cylinderParameters, drawingBuilder).Schedule(state.Dependency);
            JobHandle drawCellsOnCylinderJobHandle =
                new DrawOccupiedCellOnCylinderJob(drawingBuilder, gridParameters, cylinderParametersComponent.cylinderParameters).ScheduleParallel(
                    drawCylinderJobHandle);

            drawingBuilder.DisposeAfter(drawCellsOnCylinderJobHandle);

            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}