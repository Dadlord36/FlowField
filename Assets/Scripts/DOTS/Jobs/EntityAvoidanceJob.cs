using System.Runtime.InteropServices;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    [StructLayout(LayoutKind.Auto)]
    [BurstCompile]
    public partial struct EntityAvoidanceJob : IJobEntity
    {
        [ReadOnly] private readonly Entity _excludedEntity;
        [ReadOnly] private readonly Bounds _controlEntityBounds;
        private EntityCommandBuffer.ParallelWriter ecb;
        private Bounds _targetEntityBounds;

        public EntityAvoidanceJob(Entity excludedEntity, in Bounds controlEntityBounds, EntityCommandBuffer.ParallelWriter ecb) : this()
        {
            _excludedEntity = excludedEntity;
            _controlEntityBounds = controlEntityBounds;
            _targetEntityBounds.size = Vector3.one;
            this.ecb = ecb;
        }

        private void Execute(RefRO<LocalToWorld> locationComponent, [EntityIndexInQuery] int sortIndex, Entity entity)
        {
            if (_excludedEntity == entity) return;
            
            _targetEntityBounds.center = locationComponent.ValueRO.Position;
            bool intersects = _controlEntityBounds.Intersects(_targetEntityBounds);
            ecb.SetComponentEnabled<MovementIsBlockedTag>(sortIndex, entity, intersects);
        }
    }
}