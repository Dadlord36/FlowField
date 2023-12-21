using DOTS.Components;
using DOTS.Components.Tags;
using Drawing;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    // [DisableAutoCreation
    public partial struct DebugDrawGridSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(WalkingSurfaceTag))]
        private partial struct DrawGridJob : IJobEntity
        {
            private CommandBuilder _drawingCommandBuilder;

            public DrawGridJob(CommandBuilder drawingCommandBuilder) : this()
            {
                this._drawingCommandBuilder = drawingCommandBuilder;
            }

            public void Execute(in CylinderParametersComponent cylinderParametersComponent, in GridParametersComponent gridParametersComponent)
            {
                //Grid will be projected onto a cylinder and os will be represented by circles for rows and lines for columns.
                float2 cellSize = gridParametersComponent.gridParameters.cellSize;
                float3 cylinderCenter = cylinderParametersComponent.cylinderParameters.cylinderOrigin;
                float cylinderRadius = cylinderParametersComponent.cylinderParameters.radius;
                uint columnsNumber = gridParametersComponent.gridParameters.columnNumber; 

                // Draw rows as circles
                for (ushort i = 0; i <= gridParametersComponent.gridParameters.rowNumber; i++)
                {
                    float2 rowCenter = cylinderCenter.xy + new float2(0, i * (cellSize.y / 2));
                    _drawingCommandBuilder.xz.Circle(new float3(rowCenter, cylinderCenter.z), cylinderRadius,
                        Color.white);
                }

                // Draw columns as lines along the cylinder surface (basically they should be placed along the circle. The line should go in up direction
                for (ushort i = 0; i <= columnsNumber; i++)
                {
                    float angle = i * CylinderCalculations.TwoPi / columnsNumber;
                    float3 startPoint = new float3(math.sin(angle), 0, math.cos(angle)) * cylinderRadius;
                    float3 endPoint = startPoint + new float3(0, cylinderRadius, 0);

                    _drawingCommandBuilder.Line(startPoint, endPoint, Color.white);
                }
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridParametersComponent>();
            state.RequireForUpdate<FlowMapComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);

            state.Dependency = new DrawGridJob(drawingBuilder).Schedule(state.Dependency);

            drawingBuilder.DisposeAfter(state.Dependency);

            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}