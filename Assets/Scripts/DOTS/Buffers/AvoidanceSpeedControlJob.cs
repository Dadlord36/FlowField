using System.Runtime.InteropServices;
using DOTS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Buffers
{
    [StructLayout(LayoutKind.Auto)]
    [BurstCompile]
    public partial struct AvoidanceSpeedControlJob : IJobEntity
    {
        [ReadOnly] private readonly float _speed;

        public AvoidanceSpeedControlJob(float speed) : this()
        {
            _speed = speed;
        }

        private void Execute(RefRW<MovementSpeedComponent> movementSpeedComponent)
        {
            movementSpeedComponent.ValueRW.speed = _speed;
        }
    }
}