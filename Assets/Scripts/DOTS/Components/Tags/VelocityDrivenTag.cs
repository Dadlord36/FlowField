using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components.Tags
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    [BurstCompile]
    public struct VelocityDrivenTag : IComponentData
    {
    }
}