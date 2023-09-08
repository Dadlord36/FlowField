﻿using Structs;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct CylinderParametersComponent : IComponentData
    {
        public CylinderParameters cylinderParameters;
    }
}