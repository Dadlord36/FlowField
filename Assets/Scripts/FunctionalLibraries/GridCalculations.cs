using System.Runtime.CompilerServices;
using Structs;
using Unity.Burst;
using Unity.Mathematics;

namespace FunctionalLibraries
{
    /// <summary>
    /// Grid related calculations.
    /// </summary>
    [BurstCompile]
    public static class GridCalculations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint2 CalculateIndex2DFromIndex1D(in GridParameters gridParameters, uint index)
        {
            return new uint2(index % gridParameters.columnNumber, index / gridParameters.columnNumber);
        }
    }
}