using System.Runtime.InteropServices;
using DOTS.Components;
using DOTS.Components.Tags;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Systems.SimulationPhase
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    // [DisableAutoCreation]
    public partial struct VelocityDrivenMovementSystem : ISystem
    {
        #region Related Jobs Declaration

        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        [WithAll(typeof(SurfaceWalkerTag))]
        private partial struct VelocityDrivenMovementJob : IJobEntity
        {
            public float deltaTime;

            /*private void Execute(RefRO<VelocityComponent> velocityComponent, RefRO<MovementSpeedComponent> movementSpeedComponent,
                RefRW<LocalToWorld> localToWorld)
            {
                ref readonly LocalToWorld currentTransform = ref localToWorld.ValueRO;
                float3 newPosition = currentTransform.Position +
                                     velocityComponent.ValueRO.velocity * movementSpeedComponent.ValueRO.speed * deltaTime;

                localToWorld.ValueRW.Value = float4x4.TRS(newPosition, currentTransform.Rotation, Float3One);
            }*/

            /*private void Execute(in CylinderParametersComponent cylinderParametersComponent,
                RefRO<FlowFieldVelocityComponent> flowFieldVelocityComponent,
                RefRW<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRO<MovementSpeedComponent> movementSpeedComponent)
            {
                //Calculate translation of orbit driven by the flow velocity and speed
                /*cylinderSurfacePositioningComponent.ValueRW.orbitCoordinate = CylinderCalculations.CalculateCoordinateTranslationWithFlatVelocity(
                    cylinderSurfacePositioningComponent.ValueRO.orbitCoordinate, flowFieldVelocityComponent.ValueRO.flowVelocity,
                    movementSpeedComponent.ValueRO.speed);#1#

                cylinderSurfacePositioningComponent.ValueRW.orbitCoordinate = CylinderCalculations.ComputeNextOrbitCoordinate(
                    cylinderSurfacePositioningComponent.ValueRO.orbitCoordinate, flowFieldVelocityComponent.ValueRO.flowVelocity,
                    movementSpeedComponent.ValueRO.speed, deltaTime);
            }*/

            private void Execute(in CylinderParametersComponent cylinderParametersComponent,
                RefRO<FlowFieldVelocityComponent> flowFieldVelocityComponent,
                RefRW<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRO<MovementSpeedComponent> movementSpeedComponent)
            {
                cylinderSurfacePositioningComponent.ValueRW.surfaceCoordinate =
                    CylinderCalculations.AdjustCoordinate(cylinderSurfacePositioningComponent.ValueRO.surfaceCoordinate,
                        flowFieldVelocityComponent.ValueRO.flowVelocity * movementSpeedComponent.ValueRO.speed);
            }
        }

        [WithAll(typeof(SurfaceWalkerTag))]
        private partial struct ConvertCurrentOrbitToTransformJob : IJobEntity
        {
            /*private void Execute(in CylinderParametersComponent cylinderParametersComponent,
                RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRW<LocalToWorld> localToWorld)
            {
                localToWorld.ValueRW.Value = CylinderCalculations.OrbitToTransform(cylinderSurfacePositioningComponent.ValueRO.orbitCoordinate,
                    cylinderParametersComponent.cylinderParameters.radius);
            }*/

            private void Execute(in CylinderParametersComponent cylinderParametersComponent,
                RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRW<LocalToWorld> localToWorld)
            {
                localToWorld.ValueRW.Value = CylinderCalculations.SurfaceCoordinateToLocalToWorld(
                    cylinderSurfacePositioningComponent.ValueRO.surfaceCoordinate, cylinderParametersComponent.cylinderParameters);
            }
        }

        #endregion

        private static readonly float3 Float3One = new(1f, 1f, 1f);

        private VelocityDrivenMovementJob velocityDrivenMovementJob;
        private ConvertCurrentOrbitToTransformJob convertCurrentOrbitToTransformJob;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            /*state.RequireForUpdate<MovementSpeedComponent>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
            state.RequireForUpdate<LocalToWorld>();*/
            state.RequireForUpdate<SurfaceWalkerTag>();
            state.RequireForUpdate<VelocityDrivenTag>();

            velocityDrivenMovementJob = new VelocityDrivenMovementJob();
            convertCurrentOrbitToTransformJob = new ConvertCurrentOrbitToTransformJob();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Set delta time in velocityDrivenMovementJob
            velocityDrivenMovementJob.deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = velocityDrivenMovementJob.ScheduleParallel(state.Dependency);
            state.Dependency = convertCurrentOrbitToTransformJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}