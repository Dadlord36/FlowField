using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Structs
{
    [Serializable]
    [BurstCompile]
    public struct CylinderParameters
    {
        public float3 center;
        public float radius;
        public float height;
        [NonSerialized] public float3 cylinderOrigin;

        public CylinderParameters(float3 center, float radius, float height)
        {
            this.center = center;
            this.radius = radius;
            this.height = height;
            cylinderOrigin = center - new float3(0, height / 2, 0);
        }
    }
}