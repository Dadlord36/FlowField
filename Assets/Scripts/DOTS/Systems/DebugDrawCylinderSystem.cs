using DOTS.Components;
using DOTS.Jobs;
using Drawing;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace DOTS.Systems
{
    public partial struct DebugDrawCylinderSystem : ISystem
    {
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
                _drawingBuilder.WireCylinder(_cylinderParameters.cylinderOrigin, Vector3.up, _cylinderParameters.radius, _cylinderParameters.height,
                    Color.magenta);
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
            var gridParametersComponent = SystemAPI.GetSingleton<GridParametersComponent>();
            var cylinderParametersComponent = SystemAPI.GetSingleton<CylinderParametersComponent>();
            ref GridParameters gridParameters = ref gridParametersComponent.gridParameters;

            JobHandle drawCylinderJobHandle =
                new DrawCylinderJob(cylinderParametersComponent.cylinderParameters, drawingBuilder).Schedule(state.Dependency);
            /*JobHandle drawCellsOnCylinderJobHandle =
                new DrawOccupiedCellOnCylinderJob(drawingBuilder, gridParameters, cylinderParametersComponent.cylinderParameters).ScheduleParallel(
                    drawCylinderJobHandle);*/

            drawingBuilder.DisposeAfter(drawCylinderJobHandle);

            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}