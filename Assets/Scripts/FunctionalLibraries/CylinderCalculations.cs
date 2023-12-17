using System.Runtime.CompilerServices;
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
            in uint2 cellIndex)
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

        public static uint2 GetCellIndex2DOnCylinderSurfaceAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            float surfacePositioningHeight, float surfacePositioningAngleInRadians)
        {
            // Normalize the angle to be within [0, 2 * PI]
            surfacePositioningAngleInRadians = (surfacePositioningAngleInRadians + _twoPI) % _twoPI;

            return math.uint2(
                (uint)math.round(surfacePositioningAngleInRadians * cylinderParameters.radius / gridParameters.cellSize.x),
                (uint)math.round(surfacePositioningHeight / gridParameters.cellSize.y));
        }

        public static LocalToWorld CalculateLocalToWorldMatrixAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in uint2 cellIndex, in float3 forwardVector)
        {
            float3 pointOnCylinderSurface = GetGridCenterProjectedOnCylinderSurfaceAt(gridParameters, cylinderParameters, cellIndex);
            return CalculateLocalToWorld(pointOnCylinderSurface, forwardVector);
        }

        public static LocalToWorld GetCellCenterAtReversed(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            uint index1D, float3 forwardVector)
        {
            uint2 cellIndex2D = IndexesCalculations.CalculateReversedIndex2DFrom1D(index1D, gridParameters.rowNumber);
            return CalculateLocalToWorldMatrixAt(gridParameters, cylinderParameters, cellIndex2D, forwardVector);
        }

        public static quaternion CalculateRotationParallelToCylinderSurfaceFromAngle(in CylinderParameters cylinderParameters, float angleOnCylinder,
            float angleInDegrees)
        {
            quaternion rotation = CalculateCylinderSurfaceNormalAtAngle(cylinderParameters, angleOnCylinder);
            //Rotate the rotation by 180 degrees, because the cylinder is rotated by 90 degrees.
            rotation = math.mul(rotation, quaternion.Euler(math.radians(angleInDegrees), math.radians(90f), 0f));
            return rotation;
        }

        public static float2 Calculate2DVelocityFromDirectionAngle(float angleInDegrees)
        {
            float angleInRadians = math.radians(angleInDegrees);
            return new float2(math.cos(angleInRadians), math.sin(angleInRadians));
        }

        /*public static float3 ConvertToCylinderVelocity(in float2 flatVelocity, float angle)
        {
            // Calculate the tangent vector based on some angle (this could be dynamically determined)
            var tangentVector = new float3(-math.sin(angle), 0, math.cos(angle));

            // Calculate axial and tangential components based on flatVelocity
            float3 axialComponent = flatVelocity.y * Vector3.up; // Axial movement
            float3 tangentialVelocity = tangentVector * flatVelocity.x; // Rotational movement

            // Combine them to get the final velocity
            float3 finalVelocity = axialComponent + tangentialVelocity;

            // Scale by the original speed
            float originalSpeed = math.length(flatVelocity);
            finalVelocity *= originalSpeed;

            return finalVelocity;
        }*/

        /// <summary>
        ///  Converts a velocity in 2D space to a velocity on the surface of a cylinder at a given angle. 
        /// </summary>
        /// <param name="flatVelocity"> Velocity in 2D space. </param>
        /// <param name="angleOnCylinder"> Angle on the cylinder. </param>
        /// <returns></returns>
        public static float3 ConvertToCylinderVelocity(float2 flatVelocity, float angleOnCylinder)
        {
            // Calculate the tangent vector based on some angle (this could be dynamically determined)
            var tangentVector = new float3(-math.sin(angleOnCylinder), 0, math.cos(angleOnCylinder));

            // Calculate axial and tangential components based on flatVelocity
            float3 axialComponent = flatVelocity.y * Vector3.up; // Axial movement
            float3 tangentialVelocity = tangentVector * flatVelocity.x; // Rotational movement

            return axialComponent + tangentialVelocity;
        }

        public static LocalToWorld CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(in CylinderParameters cylinderParameters, float height,
            float angle, float3 finalForwardVector)
        {
            float3 pointOnCylinderSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
            quaternion rotation = quaternion.LookRotationSafe(finalForwardVector, Vector3.up);
            rotation = math.mul(rotation, quaternion.Euler(0f, 0f, math.radians(90f)));
            return new LocalToWorld
            {
                Value = float4x4.TRS(pointOnCylinderSurface, rotation, Vector3.one)
            };
        }

        public static float CalculateRadiansAngleOnCylinderAtPosition(in CylinderParameters cylinderParameters, float3 position)
        {
            float angle = math.atan2(position.z - cylinderParameters.center.z, position.x - cylinderParameters.center.x);
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetHeightAndAngleOnCylinderAt(in CylinderParameters cylinderParameters, float3 position, out float height,
            out float angle)
        {
            height = CalculateHeightOnCylinderAtPosition(cylinderParameters, position);
            angle = CalculateRadiansAngleOnCylinderAtPosition(cylinderParameters, position);
        }

        private static quaternion CalculateCylinderSurfaceNormalAtAngle(in CylinderParameters cylinderParameters, float angle)
        {
            float3 pointOnSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, 0);
            return quaternion.LookRotationSafe(pointOnSurface - cylinderParameters.center, Vector3.up);
        }

        private static LocalToWorld CalculateLocalToWorld(float3 pointOnCylinderSurface, float3 finalForwardVector)
        {
            quaternion rotation = quaternion.LookRotationSafe(finalForwardVector, Vector3.up);
            rotation = math.mul(rotation, quaternion.Euler(math.radians(90), 0, 0));
            return new LocalToWorld
            {
                Value = float4x4.TRS(pointOnCylinderSurface, rotation, Vector3.one)
            };
        }

        private static float CalculateHeightOnCylinderAtPosition(in CylinderParameters cylinderParameters, float3 position)
        {
            float height = position.y - cylinderParameters.cylinderOrigin.y;
            return height;
        }

        private static float CalculateRadiansAngleOnCylinderAtGridIndex(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            uint2 cellIndex)
        {
            return cellIndex.x * gridParameters.cellSize.x / cylinderParameters.radius;
        }

        private static float CalculateHeightOnCylinderAtGridIndex(in GridParameters gridParameters, uint2 cellIndex)
        {
            return cellIndex.y * gridParameters.cellSize.y;
        }
    }
}