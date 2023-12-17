using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components.Tags
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct MovementIsBlockedTag : IComponentData, IEnableableComponent
    {
    }
}