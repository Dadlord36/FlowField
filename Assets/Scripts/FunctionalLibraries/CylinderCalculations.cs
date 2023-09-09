using Structs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FunctionalLibraries
{
    public static class CylinderCalculations
    {
        private const float _twoPI = 2 * math.PI;

        /// <summary>
        /// Calculates a point on the surface of a cylinder at a given angle and height.
        /// </summary>
        /// <param name="cylinderCenter"> Cylinder center. </param>
        /// <param name="cylinderRadius"> Cylinder radius. </param>
        /// <param name="angleInRadians"> Angle in radians. </param>
        /// <param name="height"> Height of the point. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetPointOnCylinderSurfaceAt(in float3 cylinderCenter, float cylinderRadius, float angleInRadians, float height)
        {
            float x = cylinderCenter.x + cylinderRadius * math.cos(angleInRadians);
            float y = cylinderCenter.y + height;
            float z = cylinderCenter.z + cylinderRadius * math.sin(angleInRadians);
            return new float3(x, y, z);
        }

        /// <summary>
        /// Calculates a point on the surface of a cylinder at a given angle and height.
        /// </summary>
        /// <param name="cylinderParameters"> Cylinder parameters. </param>
        /// <param name="angle"> Angle in radians. </param>
        /// <param name="height"> Height of the point. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetPointOnCylinderSurfaceAt(in CylinderParameters cylinderParameters, float angle, float height)
        {
            //Asset that height is in range [0, cylinderParameters.height].
            height = math.clamp(height, 0, cylinderParameters.height);
            return GetPointOnCylinderSurfaceAt(cylinderParameters.cylinderOrigin, cylinderParameters.radius, angle, height);
        }

        /// <summary>
        ///  Calculates a point on the surface of a cylinder at given cell index.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="cylinderParameters"> Cylinder parameters. </param>
        /// <param name="cellIndex"> Cell index. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetGridCenterProjectedOnCylinderSurfaceAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in int2 cellIndex)
        {
            float angle = CalculateRadiansAngleOnCylinderAtGridIndex(gridParameters, cylinderParameters, cellIndex);
            float height = CalculateHeightOnCylinderAtGridIndex(gridParameters, cellIndex);
            return GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
        }

        // Make sure that the position is on the surface of the cylinder and not away from it (with some tolerance).
        public static bool IsOnCylinderSurface(in CylinderParameters cylinderParameters, in float3 position)
        {
            float3 cylinderCenter = cylinderParameters.center;
            float3 cylinderOrigin = cylinderParameters.cylinderOrigin;
            float3 cylinderCenterToPosition = position - cylinderCenter;
            float3 cylinderOriginToPosition = position - cylinderOrigin;
            float distanceFromCenter = math.length(cylinderCenterToPosition);
            float distanceFromOrigin = math.length(cylinderOriginToPosition);

            return math.abs(distanceFromCenter - cylinderParameters.radius) < 0.1f &&
                   math.abs(distanceFromOrigin - cylinderParameters.radius) < 0.1f;
        }

        public static int2 GetCellIndex2DOnCylinderSurfaceAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            float surfacePositioningHeight, float surfacePositioningAngleInRadians)
        {
            // Normalize the angle to be within [0, 2 * PI]
            surfacePositioningAngleInRadians = (surfacePositioningAngleInRadians + _twoPI) % _twoPI;

            return math.int2(
                (int)math.round(surfacePositioningAngleInRadians * cylinderParameters.radius / gridParameters.cellSize.x),
                (int)math.round(surfacePositioningHeight / gridParameters.cellSize.y));
        }

        public static LocalToWorld CalculateLocalToWorldMatrixAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in int2 cellIndex)
        {
            float angle = CalculateRadiansAngleOnCylinderAtGridIndex(gridParameters, cylinderParameters, cellIndex);
            float3 pointOnCylinderSurface = GetGridCenterProjectedOnCylinderSurfaceAt(gridParameters, cylinderParameters, cellIndex);
            return CalculateLocalToWorld(cylinderParameters, angle, pointOnCylinderSurface);
        }

        public static LocalToWorld CalculateLocalToWorldMatrixAt(in CylinderParameters cylinderParameters,
            float3 position)
        {
            float angle = CalculateRadiansAngleOnCylinderAtPosition(cylinderParameters, position);
            float height = CalculateHeightOnCylinderAtPosition(cylinderParameters, position);
            float3 pointOnCylinderSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
            return CalculateLocalToWorld(cylinderParameters, angle, pointOnCylinderSurface);
        }

        public static LocalToWorld CalculateLocalToWorldMatrixAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            int index1D)
        {
            int2 cellIndex2D = IndexesCalculations.CalculateIndex2DFrom1D(index1D, gridParameters.cellsRowNumber);
            return CalculateLocalToWorldMatrixAt(gridParameters, cylinderParameters, cellIndex2D);
        }

        public static LocalToWorld GetCellCenterAtReversed(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            int index1D)
        {
            int2 cellIndex2D = IndexesCalculations.CalculateReversedIndex2DFrom1D(index1D, gridParameters.cellsRowNumber);
            return CalculateLocalToWorldMatrixAt(gridParameters, cylinderParameters, cellIndex2D);
        }

        private static quaternion CalculateCylinderSurfaceNormalAtAngle(in CylinderParameters cylinderParameters, float angle)
        {
            float3 pointOnSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, 0);
            return quaternion.LookRotationSafe(pointOnSurface - cylinderParameters.center, Vector3.up);
        }

        public static LocalToWorld CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(in CylinderParameters cylinderParameters, float height,
            float angle)
        {
            float3 pointOnCylinderSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
            return CalculateLocalToWorld(cylinderParameters, angle, pointOnCylinderSurface);
        }

        private static LocalToWorld CalculateLocalToWorld(CylinderParameters cylinderParameters, float angle, float3 pointOnCylinderSurface)
        {
            quaternion rotation = CalculateCylinderSurfaceNormalAtAngle(cylinderParameters, angle);
            rotation = math.mul(rotation, quaternion.Euler(math.radians(90), 0, 0));
            return new LocalToWorld
            {
                Value = float4x4.TRS(pointOnCylinderSurface, rotation, Vector3.one)
            };
        }

        public static float CalculateRadiansAngleOnCylinderAtPosition(in CylinderParameters cylinderParameters, in float3 position)
        {
            float angle = math.atan2(position.z - cylinderParameters.center.z, position.x - cylinderParameters.center.x);
            return angle;
        }

        public static void GetHeightAndAngleOnCylinderAt(in CylinderParameters cylinderParameters, in float3 position, out float height,
            out float angle)
        {
            height = CalculateHeightOnCylinderAtPosition(cylinderParameters, position);
            angle = CalculateRadiansAngleOnCylinderAtPosition(cylinderParameters, position);
        }

        private static float CalculateHeightOnCylinderAtPosition(in CylinderParameters cylinderParameters, in float3 position)
        {
            float height = position.y - cylinderParameters.cylinderOrigin.y;
            return height;
        }

        private static float CalculateRadiansAngleOnCylinderAtGridIndex(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in int2 cellIndex)
        {
            return cellIndex.x * gridParameters.cellSize.x / cylinderParameters.radius;
        }

        private static float CalculateHeightOnCylinderAtGridIndex(in GridParameters gridParameters, in int2 cellIndex)
        {
            return cellIndex.y * gridParameters.cellSize.y;
        }
    }
}